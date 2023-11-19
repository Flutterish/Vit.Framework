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
	ColorRgba<float> tint = ColorRgba.Black;
	public ColorRgba<float> Tint {
		get => tint;
		set {
			if ( value.TrySet( ref tint ) )
				InvalidateDrawNodes();
		}
	}

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

		if ( areUniformsInitialized ) {
			drawDependencies.UniformAllocator.Free( uniforms );
			sampler!.Dispose();
		}
		if ( batches.Count != 0 ) {
			clearBatches();
		}	
	}

	struct PageBatch {
		public required uint IndexCount;
		public required uint Offset;
		public required UniformSetPool.Allocation UniformSet;
	}
	DeviceBufferHeap.Allocation<uint> indices;
	DeviceBufferHeap.Allocation<Vertex> vertices;
	List<PageBatch> batches = new();
	ISampler? sampler;

	void clearBatches () {
		foreach ( var i in batches ) {
			drawDependencies.UniformSetAllocator.Free( i.UniformSet );
		}
		indices.Dispose();
		vertices.Dispose();
		batches.Clear();
	}

	protected class DrawNode : TextDrawNode<DrawableSpriteText> {
		ColorSRgba<float> tint;
		public DrawNode ( DrawableSpriteText source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			base.UpdateState();
			tint = Source.Tint.ToSRgb();
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

			var vertexStaging = Source.drawDependencies.SingleUseBuffers.AllocateStagingBuffer<Vertex>( (uint)glyphCount * 4 );
			var indexStaging = Source.drawDependencies.SingleUseBuffers.AllocateStagingBuffer<uint>( (uint)glyphCount * 6 );
			var vertexPtr = vertexStaging.GetData();
			var indexPtr = indexStaging.GetData();
			Dictionary<SpriteFontPage, uint> groupOffsets = new( groupSizes.Count );
			var index = 0u;
			foreach ( var (page, size) in groupSizes ) {
				groupOffsets.Add( page, index );
				index += size;
			}

			foreach ( var metric in layout.AsSpan( 0, glyphCount ) ) {
				var page = store.GetSpriteFont( metric.Glyph.Font ).GetPage( metric.Glyph.Id );
				index = groupOffsets[page]++;

				indexPtr[index * 6 + 0] = index * 4 + 0;
				indexPtr[index * 6 + 1] = index * 4 + 1;
				indexPtr[index * 6 + 2] = index * 4 + 2;
				indexPtr[index * 6 + 3] = index * 4 + 0;
				indexPtr[index * 6 + 4] = index * 4 + 2;
				indexPtr[index * 6 + 5] = index * 4 + 3;

				var uv = page.GetUvBox( metric.Glyph.Id );
				var bounds = metric.Glyph.DefinedBoundingBox;

				var multiplier = (float)metric.SizeMultiplier;
				Point2<float> anchor = metric.Anchor.Cast<float>() + new Vector2<float>( (float)bounds.MinX * multiplier, (float)bounds.MinY * multiplier );
				vertexPtr[index * 4 + 0] = new() {
					UV = uv.Position + (0, uv.Height),
					Position = anchor + new Vector2<float> {
						Y = (float)bounds.Height * multiplier
					}
				};
				vertexPtr[index * 4 + 1] = new() {
					UV = uv.Position + (uv.Width, uv.Height),
					Position = anchor + new Vector2<float> {
						X = (float)bounds.Width * multiplier,
						Y = (float)bounds.Height * multiplier
					}
				};
				vertexPtr[index * 4 + 2] = new() {
					UV = uv.Position + (uv.Width, 0),
					Position = anchor + new Vector2<float> {
						X = (float)bounds.Width * multiplier
					}
				};
				vertexPtr[index * 4 + 3] = new() {
					UV = uv.Position,
					Position = anchor
				};
			}

			using var copy = renderer.CreateImmediateCommandBuffer();

			Source.vertices = Source.drawDependencies.BufferHeap.Allocate<Vertex>( BufferType.Vertex, (uint)glyphCount * 4 );
			copy.CopyBufferRaw( vertexStaging.Buffer, Source.vertices.Buffer, vertexStaging.Length, sourceOffset: vertexStaging.Offset, destinationOffset: Source.vertices.Offset );

			Source.indices = Source.drawDependencies.BufferHeap.Allocate<uint>( BufferType.Index, (uint)glyphCount * 6 );
			copy.CopyBufferRaw( indexStaging.Buffer, Source.indices.Buffer, indexStaging.Length, sourceOffset: indexStaging.Offset, destinationOffset: Source.indices.Offset );

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

			if ( ValidateLayout() )
				updateTextMesh( renderer );
			if ( Source.batches.Count == 0 )
				return;

			commands.SetShaders( shaders );
			uniforms.Buffer.UploadUniform( new Uniforms {
				Matrix = new( Matrix3<float>.CreateScale( (float)MetricMultiplier, (float)MetricMultiplier ) * UnitToGlobalMatrix ),
				Tint = tint
			}, uniforms.Offset );
			
			commands.BindVertexBufferRaw( Source.vertices.Buffer, offset: Source.vertices.Offset );
			commands.BindIndexBufferRaw( Source.indices.Buffer, IndexBufferType.UInt32, Source.indices.Offset );
			foreach ( var batch in Source.batches ) {
				shaders.SetUniformSet( batch.UniformSet.UniformSet, set: 1 );
				commands.UpdateUniforms();
				commands.DrawIndexed( batch.IndexCount, batch.Offset );
			}
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
