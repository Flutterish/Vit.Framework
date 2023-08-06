using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test04_Samplers : GenericRenderThread {
	public Test04_Samplers ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	IShaderPart vertex = null!;
	IShaderPart fragment = null!;
	IShaderSet shaderSet = null!;

	struct Vertex {
		public Point2<float> Position;
		public Point2<float> UV;
	}

	struct Uniforms {
		public Matrix4<float> ModelMatrix;
	}

	IDeviceBuffer<Vertex> positions = null!;
	IDeviceBuffer<uint> indices = null!;
	IHostBuffer<Uniforms> uniformBuffer = null!;
	ITexture texture = null!;
	protected override bool Initialize () {
		if ( !base.Initialize() )
			return false;

		vertex = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inPosition;
			layout(location = 1) in vec2 inUv;

			layout(location = 0) out vec2 outUv;

			layout(binding = 0) uniform Uniforms {
				mat4 model;
			} uniforms;

			void main () {
				outUv = inUv;
				gl_Position = uniforms.model * vec4(inPosition, 0, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
		fragment = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inUv;

			layout(location = 0) out vec4 outColor;

			layout(binding = 1) uniform sampler2D texSampler;

			void main () {
				outColor = texture( texSampler, inUv );
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );
		shaderSet = Renderer.CreateShaderSet( new[] { vertex, fragment } );

		positions = Renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
		indices = Renderer.CreateDeviceBuffer<uint>( BufferType.Index );
		uniformBuffer = Renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );
		using var image = Image.Load<Rgba32>( "./texture.jpg" );
		image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		texture = Renderer.CreateTexture( new( (uint)image.Size.Width, (uint)image.Size.Height ), PixelFormat.RGBA32 );

		positions.Allocate( 4, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
		indices.Allocate( 6, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
		uniformBuffer.Allocate( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.GpuPerFrame );

		shaderSet.SetUniformBuffer( uniformBuffer );
		shaderSet.SetSampler( texture, 1 );
		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			commands.Upload( positions, new Vertex[] {
				new() { Position = new( -0.5f, 0.5f ), UV = new( 0, 1 ) },
				new() { Position = new( 0.5f, 0.5f ), UV = new( 1, 1 ) },
				new() { Position = new( 0.5f, -0.5f ), UV = new( 1, 0 ) },
				new() { Position = new( -0.5f, -0.5f ), UV = new( 0, 0 ) }
			} );
			commands.Upload( indices, new uint[] {
				0, 1, 2,
				0, 2, 3
			} );

			if ( !image.DangerousTryGetSinglePixelMemory( out var memory ) )
				throw new Exception( "Oops, cant load image" );
			commands.UploadTextureData<Rgba32>( texture, memory.Span );
		}

		return true;
	}

	DateTime start = DateTime.Now;
	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer, clearColor: new ColorHsv<Radians<float>, float> {
			H = ((float)(DateTime.Now - start).TotalSeconds).Radians(),
			S = 1,
			V = 1
		}.ToRgba(), clearDepth: 1 );
		commands.SetShaders( shaderSet );
		commands.SetViewport( framebuffer.Size );
		commands.SetScissors( framebuffer.Size );

		commands.BindVertexBuffer( positions );
		commands.BindIndexBuffer( indices );
		uniformBuffer.Upload( new Uniforms {
			ModelMatrix = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitY, ((float)(DateTime.Now - start).TotalSeconds * 50).Degrees() )
				* Matrix4<float>.CreateTranslation( 0, 0, 1.2f )
				* Renderer.CreateLeftHandCorrectionMatrix<float>()
				* Matrix4<float>.CreatePerspective( 1, 1, 0.01f, 100f )
		} );

		commands.SetTopology( Topology.Triangles );
		commands.DrawIndexed( 6 );
	}

	protected override void Dispose () {
		indices.Dispose();
		positions.Dispose();
		uniformBuffer.Dispose();
		texture.Dispose();

		shaderSet.Dispose();
		vertex.Dispose();
		fragment.Dispose();
	}
}
