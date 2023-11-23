using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vit.Framework.Text.Layout;
using Vit.Framework.TwoD.Rendering.Shaders;
using Uniforms = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Uniforms;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public class DrawableStencilText : DrawableText {
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

	Shader shader = null!;
	Texture texture = null!;
	StencilFontStore stencilFont = null!;
	SingleUseBufferSectionStack singleUseBuffers = null!;
	DeviceBufferHeap bufferHeap = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );

		shader = deps.Resolve<ShaderStore>().GetShader( new() {
			Vertex = new() {
				Shader = BasicVertex.Identifier,
				Input = BasicVertex.InputDescription
			},
			Fragment = BasicFragment.Identifier
		} );
		texture = deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
		stencilFont = deps.Resolve<StencilFontStore>();
		singleUseBuffers = deps.Resolve<SingleUseBufferSectionStack>();
		bufferHeap = deps.Resolve<DeviceBufferHeap>();
	}

	protected override DrawNode CreateDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	protected override void Dispose ( bool disposing ) {
		base.Dispose( disposing );
	}

	IUniformSet? uniformSet;
	DeviceBufferHeap.Allocation<uint> indices;
	DeviceBufferHeap.Allocation<Vertex> vertices;
	IHostBuffer<Uniforms>? uniforms;
	int indexCount;

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();

		if ( uniformSet != null ) {
			uniformSet!.Dispose();
			uniforms!.Dispose();
		}
		if ( indexCount != 0 ) {
			indices.Dispose();
			vertices.Dispose();
		}
	}

	public class DrawNode : TextDrawNode<DrawableStencilText> {
		public DrawNode ( DrawableStencilText source, int subtreeIndex ) : base( source, subtreeIndex ) {
			stencilFont = source.stencilFont;
		}

		Shader shader = null!;
		StencilFontStore stencilFont;
		Texture texture = null!;
		ColorSRgba<float> tint;
		protected override void UpdateState () {
			base.UpdateState();
			shader = Source.shader;
			texture = Source.texture;
			tint = Source.Tint.WithOpacity( Source.alpha ).ToSRgb();
		}

		unsafe void updateTextMesh ( IRenderer renderer ) {
			var shaders = shader.Value;
			ref var indices = ref Source.indices;
			ref var vertices = ref Source.vertices;

			if ( Source.indexCount != 0 ) {
				indices.Dispose();
				vertices.Dispose();
			}

			using var layout = new RentedArray<GlyphMetric>( SingleLineTextLayoutEngine.GetBufferLengthFor( Text ) );
			var glyphCount = SingleLineTextLayoutEngine.ComputeLayout( Text, Font, layout, out var bounds );

			int vertexCount = 0;
			int indexCount = 0;
			foreach ( var i in layout.AsSpan( 0, glyphCount ) ) {
				var glyph = stencilFont.GetGlyph( i.Glyph );
				vertexCount += glyph.Vertices.Count;
				indexCount += glyph.Indices.Count;
			}

			Source.indexCount = indexCount;
			if ( Source.indexCount == 0 )
				return;

			ref var uniformSet = ref Source.uniformSet;
			if ( uniformSet == null ) {
				ref var uniforms = ref Source.uniforms;
				uniforms = renderer.CreateUniformHostBuffer<Uniforms>( 1, BufferType.Uniform, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame ); // TODO no need to reallocate the uniforms
				uniformSet = shaders.CreateUniformSet( set: 1 );

				uniformSet.SetUniformBuffer( uniforms, binding: 0 );
				uniformSet.SetSampler( texture.View, texture.Sampler, binding: 1 );
			}

			uint vertexOffset = 0;
			var indexStaging = Source.singleUseBuffers.AllocateStagingBuffer<uint>( (uint)indexCount );
			var vertexStaging = Source.singleUseBuffers.AllocateStagingBuffer<Vertex>( (uint)vertexCount );
			var indexPtr = indexStaging.GetData();
			var vertexPtr = vertexStaging.GetData();
			foreach ( var i in layout.AsSpan( 0, glyphCount ) ) {
				var glyph = stencilFont.GetGlyph( i.Glyph );
				var advance = i.Anchor.Cast<float>().FromOrigin();

				foreach ( var index in glyph.Indices ) {
					*indexPtr = index + vertexOffset;
					indexPtr++;
				}

				foreach ( var vertex in glyph.Vertices ) {
					*vertexPtr = new() { PositionAndUV = vertex + advance };
					vertexPtr++;
					vertexOffset++;
				}
			}

			indices = Source.bufferHeap.Allocate<uint>( BufferType.Index, (uint)indexCount );
			vertices = Source.bufferHeap.Allocate<Vertex>( BufferType.Vertex, (uint)vertexCount );

			using var copy = renderer.CreateImmediateCommandBuffer();
			copy.CopyBufferRaw( indexStaging.Buffer, indices.Buffer, indexStaging.Length, sourceOffset: indexStaging.Offset, destinationOffset: indices.Offset );
			copy.CopyBufferRaw( vertexStaging.Buffer, vertices.Buffer, vertexStaging.Length, sourceOffset: vertexStaging.Offset, destinationOffset: vertices.Offset );
		}

		public override void Draw ( ICommandBuffer commands ) {
			var shaders = shader.Value;
			ref var indices = ref Source.indices;
			ref var vertices = ref Source.vertices;
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			var renderer = commands.Renderer;

			if ( ValidateLayout() )
				updateTextMesh( renderer );
			if ( Source.indexCount == 0 )
				return;

			shaders.SetUniformSet( uniformSet!, set: 1 );

			commands.SetShaders( shaders );
			commands.BindVertexBufferRaw( vertices.Buffer, offset: vertices.Offset );
			commands.BindIndexBufferRaw( indices.Buffer, IndexBufferType.UInt32, indices.Offset );
			uniforms!.UploadUniform( new Uniforms {
				Matrix = new( Matrix3<float>.CreateScale( (float)MetricMultiplier, (float)MetricMultiplier ) * UnitToGlobalMatrix ),
				Tint = tint
			} );

			using ( commands.PushDepthTest( new( CompareOperation.Never ), new() { WriteOnPass = false } ) ) {
				using ( commands.PushStencilTest( new( CompareOperation.Always ), new() {
					CompareMask = 1u,
					WriteMask = 1u,
					DepthFailOperation = StencilOperation.Invert
				} ) ) {
					commands.DrawIndexed( (uint)Source.indexCount );
				}
			}

			using ( commands.PushStencilTest( new( CompareOperation.Equal ), new( StencilOperation.SetTo0 ) { CompareMask = 1u, ReferenceValue = 1u } ) ) {
				commands.DrawIndexed( (uint)Source.indexCount );
			}
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
