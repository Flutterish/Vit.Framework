using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vit.Framework.Text.Layout;
using Uniforms = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Uniforms;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public partial class DrawableSpriteText : DrawableText {
	protected override DrawNode CreateDrawNode<TRenderer> ( int subtreeIndex ) {
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

		if ( !areUniformsInitialized )
			return;

		clearBatches();
		sampler!.Dispose();
		drawDependencies.UniformAllocator.Free( uniforms );
	}

	struct PageBatch {
		public required uint IndexCount;
		public required uint Offset;
		public required UniformSetPool.Allocation UniformSet;
	}
	IDeviceBuffer<uint> indices = null!;
	IDeviceBuffer<Vertex> vertices = null!;
	List<PageBatch> batches = new();
	ISampler? sampler;

	void clearBatches () {
		foreach ( var i in batches ) {
			drawDependencies.UniformSetAllocator.Free( i.UniformSet );
		}
		indices?.Dispose();
		vertices?.Dispose();
		batches.Clear();
	}

	protected class DrawNode : TextDrawNode<DrawableSpriteText> {
		public DrawNode ( DrawableSpriteText source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		void initializeSharedData ( IRenderer renderer ) {
			ref var uniforms = ref Source.uniforms;

			uniforms = Source.drawDependencies.UniformAllocator.Allocate();
			Source.sampler = renderer.CreateSampler();
		}

		unsafe void updateTextMesh ( IRenderer renderer ) {
			Source.clearBatches();
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

			var vertexStaging = Source.drawDependencies.SingleUseBuffers.AllocateStagingBuffer<Vertex>( (uint)glyphCount * 4 );
			var indexStaging = Source.drawDependencies.SingleUseBuffers.AllocateStagingBuffer<Vertex>( (uint)glyphCount * 6 );
			var verticesList = vertexStaging.GetData<Vertex>();
			var indicesList = indexStaging.GetData<uint>();
			Dictionary<SpriteFontPage, uint> groupOffsets = new( groupSizes.Count );
			var index = 0u;
			foreach ( var (page, size) in groupSizes ) {
				groupOffsets.Add( page, index );
				index += size;
			}

			foreach ( var metric in layout.AsSpan( 0, glyphCount ) ) {
				var page = store.GetSpriteFont( metric.Glyph.Font ).GetPage( metric.Glyph.Id );
				index = groupOffsets[page]++;

				indicesList[index * 6 + 0] = index * 4 + 0;
				indicesList[index * 6 + 1] = index * 4 + 1;
				indicesList[index * 6 + 2] = index * 4 + 2;
				indicesList[index * 6 + 3] = index * 4 + 0;
				indicesList[index * 6 + 4] = index * 4 + 2;
				indicesList[index * 6 + 5] = index * 4 + 3;

				var uv = page.GetUvBox( metric.Glyph.Id );
				var bounds = metric.Glyph.DefinedBoundingBox;

				var multiplier = (float)metric.SizeMultiplier;
				Point2<float> anchor = metric.Anchor.Cast<float>() + new Vector2<float>( (float)bounds.MinX * multiplier, (float)bounds.MinY * multiplier );
				verticesList[index * 4 + 0] = new() {
					UV = uv.Position + (0, uv.Height),
					Position = anchor + new Vector2<float> {
						Y = (float)bounds.Height * multiplier
					}
				};
				verticesList[index * 4 + 1] = new() {
					UV = uv.Position + (uv.Width, uv.Height),
					Position = anchor + new Vector2<float> {
						X = (float)bounds.Width * multiplier,
						Y = (float)bounds.Height * multiplier
					}
				};
				verticesList[index * 4 + 2] = new() {
					UV = uv.Position + (uv.Width, 0),
					Position = anchor + new Vector2<float> {
						X = (float)bounds.Width * multiplier
					}
				};
				verticesList[index * 4 + 3] = new() {
					UV = uv.Position,
					Position = anchor
				};
			}

			ref var uniforms = ref Source.uniforms;

			using var copy = renderer.CreateImmediateCommandBuffer();

			Source.vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
			Source.vertices.Allocate( vertexStaging.Length, BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
			copy.CopyBufferRaw( vertexStaging.Buffer, Source.vertices, vertexStaging.Length, sourceOffset: vertexStaging.Offset );

			Source.indices = renderer.CreateDeviceBuffer<uint>( BufferType.Index );
			Source.indices.Allocate( indexStaging.Length, BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
			copy.CopyBufferRaw( indexStaging.Buffer, Source.indices, indexStaging.Length, sourceOffset: indexStaging.Offset );

			foreach ( var (page, offset) in groupOffsets ) {
				var size = groupSizes[page];

				var uniformSet = Source.drawDependencies.UniformSetAllocator.Allocate();
				uniformSet.UniformSet.SetUniformBuffer( uniforms.Buffer, binding: 0, uniforms.Offset );
				uniformSet.UniformSet.SetSampler( page.View, Source.sampler!, binding: 1 );

				var batch = new PageBatch {
					IndexCount = size * 6,
					UniformSet = uniformSet,
					Offset = (offset - size) * 6
				};
				Source.batches.Add( batch );
			}
		}

		public override void Draw ( ICommandBuffer commands ) {
			ref var uniforms = ref Source.uniforms;
			var renderer = commands.Renderer;

			var shaders = Source.drawDependencies.Shader;

			if ( !Source.areUniformsInitialized ) {
				initializeSharedData( renderer );
				Source.areUniformsInitialized = true;
			}

			if ( ValidateLayout() )
				updateTextMesh( renderer );
			if ( Source.batches.Count == 0 )
				return;

			commands.SetShaders( shaders );
			uniforms.Buffer.UploadUniform( new Uniforms {
				Matrix = new( Matrix3<float>.CreateScale( (float)MetricMultiplier, (float)MetricMultiplier ) * UnitToGlobalMatrix ),
				Tint = ColorSRgba.White
			}, uniforms.Offset );
			
			commands.BindVertexBuffer( Source.vertices );
			commands.BindIndexBuffer( Source.indices );
			foreach ( var batch in Source.batches ) {
				shaders.SetUniformSet( batch.UniformSet.UniformSet, set: 1 );
				commands.UpdateUniforms();
				commands.DrawIndexed( batch.IndexCount, batch.Offset );
			}
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
