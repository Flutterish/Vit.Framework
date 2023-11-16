using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Textures;
using Uniforms = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Uniforms;

namespace Vit.Framework.TwoD.Graphics;

public abstract partial class TexturedQuad : Drawable {
	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );

		drawDependencies = deps.Resolve<DrawDependencies>();
	}

	ColorRgba<float> tint = ColorRgba.White;
	public ColorRgba<float> Tint {
		get => tint;
		set {
			if ( tint == value )
				return;

			tint = value;
			InvalidateDrawNodes();
		}
	}

	DrawDependencies drawDependencies = null!;
	bool areUniformsInitialized = false;
	UniformSetPool.Allocation uniformSet;
	BufferSectionPool<IHostBuffer<Uniforms>>.Allocation uniforms;

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();

		if ( !areUniformsInitialized )
			return;

		drawDependencies.UniformSetAllocator.Free( uniformSet );
		drawDependencies.UniformAllocator.Free( uniforms );
	}

	public abstract class DrawNode<T> : DrawableDrawNode<T> where T : TexturedQuad {
		public DrawNode ( T source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		ColorSRgba<float> tint;
		protected override void UpdateState () {
			base.UpdateState();
			tint = Source.tint.ToSRgb();
		}

		void initializeSharedData () {
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			uniforms = Source.drawDependencies.UniformAllocator.Allocate();
			uniformSet = Source.drawDependencies.UniformSetAllocator.Allocate();

			uniformSet.UniformSet.SetUniformBuffer( uniforms.Buffer, binding: 0, uniforms.Offset );
		}

		protected abstract bool UpdateTexture ( [NotNullWhen( true )] out ITexture2DView? texture, [NotNullWhen( true )] out ISampler? sampler );

		public override void Draw ( ICommandBuffer commands ) {
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet.UniformSet;

			var shaders = Source.drawDependencies.Shader;

			if ( !Source.areUniformsInitialized ) {
				initializeSharedData();
				Source.areUniformsInitialized = true;
			}

			var indices = Source.drawDependencies.Indices;
			var vertices = Source.drawDependencies.Vertices;

			if ( UpdateTexture( out var texture, out var sampler ) ) {
				uniformSet.SetSampler( texture, sampler, binding: 1 );
			}
			
			shaders.SetUniformSet( uniformSet, set: 1 );

			commands.SetShaders( shaders );
			commands.BindVertexBuffer( vertices );
			commands.BindIndexBuffer( indices );
			uniforms.Buffer.UploadUniform( new Uniforms { // in theory we could make the matrix and tint per-instance to both save space on uniform buffers and batch sprites
				Matrix = new( UnitToGlobalMatrix ),
				Tint = tint
			}, uniforms.Offset );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
