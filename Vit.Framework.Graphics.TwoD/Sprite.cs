using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public class Sprite : Drawable {
	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	new public class DrawNode : BasicDrawNode<Sprite> {
		public DrawNode ( Sprite source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		struct Vertex {
			public Point2<float> PositionAndUV;
		}

		struct Uniforms {
			public Matrix3<float> Matrix;
		}

		IShaderPart? vertex;
		IShaderPart? fragment;
		IShaderSet? shaders;
		IDeviceBuffer<ushort>? indices;
		IDeviceBuffer<Vertex>? vertices;
		IHostBuffer<Uniforms>? uniforms;
		public override void Draw ( ICommandBuffer commands ) {
			if ( indices == null ) {
				var renderer = commands.Renderer;
				using var copy = renderer.CreateImmediateCommandBuffer();
				indices = renderer.CreateDeviceBuffer<ushort>( BufferType.Index );
				indices.Allocate( 6, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.Upload( indices, new ushort[] {
					0, 1, 2,
					0, 2, 3
				} );
				vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
				vertices.Allocate( 4, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.Upload( vertices, new Vertex[] {
					new() { PositionAndUV = new( 0, 1 ) },
					new() { PositionAndUV = new( 1, 1 ) },
					new() { PositionAndUV = new( 1, 0 ) },
					new() { PositionAndUV = new( 0, 0 ) }
				} );
				uniforms = renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );
				uniforms.Allocate( 1, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );

				vertex = renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
					layout(location = 0) in vec2 inPositionAndUv;

					layout(location = 0) out vec2 outUv;

					layout(binding = 0) uniform Uniforms {
						mat3 model;
					} uniforms;

					void main () {
						outUv = inPositionAndUv;
						gl_Position = vec4((uniforms.model * vec3(inPositionAndUv, 1)).xy, 0, 1);
					}
				", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
				fragment = renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
					layout(location = 0) in vec2 inUv;

					layout(location = 0) out vec4 outColor;

					//layout(binding = 1) uniform sampler2D texSampler;

					void main () {
						outColor = vec4(inUv, 0, 1);//texture( texSampler, inUv );
					}
				", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );
				shaders = renderer.CreateShaderSet( new[] { vertex, fragment } );

				shaders.SetUniformBuffer( uniforms, binding: 0 );
			}

			commands.SetShaders( shaders! );
			commands.BindVertexBuffer( vertices! );
			commands.BindIndexBuffer( indices! );
			uniforms!.Upload( new Uniforms { Matrix = UnitToGlobalMatrix * commands.Renderer.CreateLeftHandCorrectionMatrix<float>().ToMatrix3() } );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) {
			if ( indices == null )
				return;

			indices!.Dispose();
			vertices!.Dispose();
			uniforms!.Dispose();
			shaders!.Dispose();
			vertex!.Dispose();
			fragment!.Dispose();

			indices = null;
			vertices = null;
			uniforms = null;
			shaders = null;
			vertex = null;
			fragment = null;
		}
	}
}
