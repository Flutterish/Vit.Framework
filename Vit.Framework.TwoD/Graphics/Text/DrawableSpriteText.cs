using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders.Types;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vit.Framework.Text.Layout;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Uniforms = Vit.Framework.TwoD.Rendering.Shaders.TextVertex.Uniforms;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.TextVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public partial class DrawableSpriteText : DrawableText {
	ColorRgb<float> tint = ColorRgb.Black;
	public ColorRgb<float> Tint {
		get => tint;
		set {
			if ( value.TrySet( ref tint ) )
				InvalidateDrawNodes();
		}
	}

	float alpha = 1f;
	public float Alpha {
		get => alpha;
		set {
			if ( value.TrySet( ref alpha ) )
				InvalidateDrawNodes();
		}
	}

	protected override DrawNode CreateDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );

		drawDependencies = dependencies.Resolve<DrawDependencies>();
	}

	DrawDependencies drawDependencies = null!;
	bool areUniformsInitialized = false;
	BufferSectionPool<IHostBuffer<Uniforms>>.Allocation uniforms;

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();

		if ( areUniformsInitialized ) {
			drawDependencies.UniformAllocator.Free( uniforms );
			sampler!.Dispose();
			areUniformsInitialized = false;
		}
		if ( batches.Count != 0 ) {
			clearBatches();
		}
	}

	struct PageBatch {
		public required uint InstanceOffset;
		public required uint InstanceCount;
		public required UniformSetPool.Allocation UniformSet;
	}
	DeviceBufferHeap.Allocation<Vertex> vertices;
	List<PageBatch> batches = new();
	ISampler? sampler;

	void clearBatches () {
		foreach ( var i in batches ) {
			drawDependencies.UniformSetAllocator.Free( i.UniformSet );
		}
		vertices.Dispose();
		batches.Clear();
	}

	protected class DrawNode : TextDrawNode<DrawableSpriteText> {
		ColorSRgba<float> tint;
		public DrawNode ( DrawableSpriteText source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			base.UpdateState();
			tint = Source.Tint.WithOpacity( Source.Alpha ).ToSRgb();
		}

		unsafe void updateTextMesh ( IRenderer renderer ) {
			if ( Source.batches.Count != 0 ) {
				Source.clearBatches();
			}

			using var layout = new RentedArray<GlyphMetric>( SingleLineTextLayoutEngine.GetBufferLengthFor( Text ) );
			var glyphCount = SingleLineTextLayoutEngine.ComputeLayout( Text, Font, layout, out var textBounds );

			var store = Source.drawDependencies.Store;

			Dictionary<SpriteFontPage, uint> groupSizes = new();
			foreach ( var metric in layout.AsSpan( 0, glyphCount ) ) {
				var page = store.GetSpriteFont( metric.Glyph.Font ).GetPage( metric.Glyph.Id );
				if ( groupSizes.ContainsKey( page ) ) {
					groupSizes[page]++;
				}
				else {
					groupSizes[page] = 1;
				}
			}

			if ( groupSizes.Count == 0 )
				return;

			ref var uniforms = ref Source.uniforms;
			if ( !Source.areUniformsInitialized ) {
				uniforms = Source.drawDependencies.UniformAllocator.Allocate();
				Source.sampler = renderer.CreateSampler();
				Source.areUniformsInitialized = true;
			}

			var vertexStaging = Source.drawDependencies.SingleUseBuffers.AllocateStagingBuffer<Vertex>( (uint)glyphCount );
			var vertexPtr = vertexStaging.GetData();
			Dictionary<SpriteFontPage, uint> groupOffsets = new( groupSizes.Count );
			var instanceIndex = 0u;
			foreach ( var (page, size) in groupSizes ) {
				groupOffsets.Add( page, instanceIndex );
				instanceIndex += size;
			}

			foreach ( var metric in layout.AsSpan( 0, glyphCount ) ) {
				var page = store.GetSpriteFont( metric.Glyph.Font ).GetPage( metric.Glyph.Id );
				instanceIndex = groupOffsets[page]++;

				var uv = page.GetUvBox( metric.Glyph.Id );
				var bounds = metric.Glyph.DefinedBoundingBox;

				var multiplier = (float)metric.SizeMultiplier;
				Point2<float> anchor = metric.Anchor.Cast<float>() + new Vector2<float>( (float)bounds.MinX * multiplier, (float)bounds.MinY * multiplier );
				var rectangle = new UniformRectangle<float>( anchor, bounds.Size.Cast<float>() * multiplier );
				vertexPtr[instanceIndex] = new() {
					UvRectangle = uv,
					Rectangle = rectangle,
					IgnoreTint = page.HasOwnColor( metric.Glyph.Id )
				};
			}

			using var copy = renderer.CreateImmediateCommandBuffer();

			Source.vertices = Source.drawDependencies.BufferHeap.Allocate<Vertex>( BufferType.Vertex, (uint)glyphCount * 4 );
			copy.CopyBufferRaw( vertexStaging.Buffer, Source.vertices.Buffer, vertexStaging.Length, sourceOffset: vertexStaging.Offset, destinationOffset: Source.vertices.Offset );

			foreach ( var (page, offset) in groupOffsets ) {
				var size = groupSizes[page];

				var uniformSet = Source.drawDependencies.UniformSetAllocator.Allocate();
				uniformSet.UniformSet.SetUniformBuffer( uniforms.Buffer, binding: 0, uniforms.Offset );
				uniformSet.UniformSet.SetSampler( page.View, Source.sampler!, binding: 1 );

				var batch = new PageBatch {
					InstanceOffset = offset - size,
					InstanceCount = size,
					UniformSet = uniformSet
				};
				Source.batches.Add( batch );
			}
		}

		public override void Draw ( ICommandBuffer commands ) {
			ref var uniforms = ref Source.uniforms;
			var renderer = commands.Renderer;

			var shaders = Source.drawDependencies.Shader;

			if ( ValidateLayout() )
				updateTextMesh( renderer );
			if ( Source.batches.Count == 0 )
				return;

			commands.SetShaders( shaders );
			uniforms.Buffer.UploadUniform( new Uniforms {
				Matrix = new( Matrix3<float>.CreateScale( (float)MetricMultiplier, (float)MetricMultiplier ) * UnitToGlobalMatrix ),
				Tint = tint,
				MaskingPointer = Source.drawDependencies.Masking.MaskPointer
			}, uniforms.Offset );
			
			commands.BindVertexBufferRaw( Source.vertices.Buffer, offset: Source.vertices.Offset, binding: 0 );
			commands.BindVertexBufferRaw( Source.drawDependencies.CornerBuffer, binding: 1 );
			commands.BindIndexBuffer( Source.drawDependencies.IndexBuffer );

			foreach ( var batch in Source.batches ) {
				shaders.SetUniformSet( batch.UniformSet.UniformSet, set: 1 );
				commands.UpdateUniforms();
				commands.DrawInstancesIndexed( 6, batch.InstanceCount, instanceOffset: batch.InstanceOffset );
			}
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
