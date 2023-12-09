using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.TwoD.Rendering;
using InstanceData = Vit.Framework.TwoD.Rendering.Shaders.MaskedVertex.InstanceData;

namespace Vit.Framework.TwoD.Graphics;

public abstract partial class TexturedQuad : Drawable {
	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );

		drawDependencies = deps.Resolve<DrawDependencies>();
	}

	ColorRgb<float> tint = ColorRgb.White;
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

	DrawDependencies drawDependencies = null!;

	interface ITexturedQuadDrawNode {
		(ITexture2DView, ISampler) GetTextureSampler ( IRenderer renderer );
		DrawDependencies Dependencies { get; }
		InstanceData GetInstanceData ();
	}
	public abstract class DrawNode<T> : DrawableDrawNode<T>, ITexturedQuadDrawNode where T : TexturedQuad {
		public override DrawNodeBatchContract BatchContract => TexturedQuadBatchContract.Instance;
		public DrawNode ( T source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		ColorSRgba<float> tint;
		protected override void UpdateState () {
			base.UpdateState();
			tint = Source.tint.WithOpacity( Source.alpha ).ToSRgb();
		}

		public override void Draw ( ICommandBuffer commands ) { }

		public abstract (ITexture2DView, ISampler) GetTextureSampler ( IRenderer renderer );
		public DrawDependencies Dependencies => Source.drawDependencies;
		public InstanceData GetInstanceData () => new() {
			Matrix = UnitToGlobalMatrix,
			Tint = tint,
			MaskingPointer = Source.drawDependencies.Masking.MaskPointer
		};

		public override void ReleaseResources ( bool willBeReused ) { }
	}

	class TexturedQuadBatchContract : DrawNodeBatchContract<ITexturedQuadDrawNode> {
		public static readonly TexturedQuadBatchContract Instance = new();

		public override unsafe void Draw ( ICommandBuffer commands, ReadOnlySpan<ITexturedQuadDrawNode> drawNodes ) {
			var first = drawNodes[0];
			var deps = first.Dependencies;
			var vertex = deps.Vertices;
			var indices = deps.Indices;
			var shader = deps.Shader;

			var instance = deps.BatchAllocator.AllocateHostBuffer<InstanceData>( (uint)drawNodes.Length, BufferType.Vertex, BufferUsage.CpuWrite );
			var dataPtr = instance.Map();

			commands.SetShaders( shader );
			commands.BindIndexBuffer( indices );
			commands.BindVertexBuffer( vertex, binding: 0 );
			commands.BindVertexBufferRaw( instance.Buffer, binding: 1, offset: instance.Offset );

			uint length = 0;
			uint instanceOffset = 0;
			var renderder = commands.Renderer;
			var textureSampler = first.GetTextureSampler( renderder );

			void draw () {
				var set = deps.UniformSetAllocator.Allocate();

				set.SetSampler( textureSampler.Item1, textureSampler.Item2, binding: 0 );

				shader.SetUniformSet( set, set: 1 );
				commands.UpdateUniforms();

				commands.DrawInstancesIndexed( 6, length, instanceOffset: instanceOffset );
			}

			foreach ( var i in drawNodes ) {
				*dataPtr = i.GetInstanceData();
				dataPtr++;

				var nextTextureSampler = i.GetTextureSampler( renderder );
				if ( nextTextureSampler != textureSampler ) {
					draw();
					textureSampler = nextTextureSampler;
					instanceOffset += length;
					length = 0;
				}

				length++;
			}

			draw();
		}
	}
}
