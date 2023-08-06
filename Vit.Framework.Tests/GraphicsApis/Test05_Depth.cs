using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Parsing.WaveFront;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test05_Depth : GenericRenderThread {
	public Test05_Depth ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	IShaderPart vertex = null!;
	IShaderPart fragment = null!;
	IShaderSet shaderSet = null!;

	struct Vertex {
		public Point3<float> Position;
		public Point2<float> UV;
	}

	struct Uniforms {
		public Matrix4<float> ModelMatrix;
	}

	uint indexCount;
	IDeviceBuffer<Vertex> positions = null!;
	IDeviceBuffer<uint> indices = null!;
	IHostBuffer<Uniforms> uniformBuffer = null!;
	ITexture texture = null!;
	protected override bool Initialize () {
		if ( !base.Initialize() )
			return false;

		vertex = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec3 inPosition;
			layout(location = 1) in vec2 inUv;

			layout(location = 0) out vec2 outUv;

			layout(binding = 0) uniform Uniforms {
				mat4 model;
			} uniforms;

			void main () {
				outUv = inUv;
				gl_Position = uniforms.model * vec4(inPosition, 1);
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
		using var image = Image.Load<Rgba32>( "./viking_room.png" );
		image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		texture = Renderer.CreateTexture( new( (uint)image.Size.Width, (uint)image.Size.Height ), PixelFormat.RGBA32 );

		var model = SimpleObjModel.FromLines( File.ReadLines( "./viking_room.obj" ) );
		positions.Allocate( (uint)model.Vertices.Count, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
		indices.Allocate( indexCount = (uint)model.Indices.Count, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
		uniformBuffer.Allocate( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.GpuPerFrame );

		shaderSet.SetUniformBuffer( uniformBuffer );
		shaderSet.SetSampler( texture, 1 );
		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			commands.Upload( positions, model.Vertices.Select( x => new Vertex {
				Position = x.Position.XYZ,
				UV = x.TextureCoordinates.XY
			} ).ToArray() );
			commands.Upload( indices, model.Indices.AsSpan() );

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
			ModelMatrix = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitX, -90f.Degrees() )
				* Matrix4<float>.CreateTranslation( 0, -0.3f, 0 )
				* Matrix4<float>.FromAxisAngle( Vector3<float>.UnitY, ((float)(DateTime.Now - start).TotalSeconds * 5).Degrees() )
				* Matrix4<float>.CreateTranslation( 0, 0, 1.2f )
				* Renderer.CreateLeftHandCorrectionMatrix<float>()
				* Matrix4<float>.CreatePerspective( Window.Size.Width, Window.Size.Height, 0.01f, 100f )
		} );

		commands.SetTopology( Topology.Triangles );
		commands.SetDepthTest( new( CompareOperation.LessThan ), new() { WriteOnPass = true } );
		commands.DrawIndexed( indexCount );
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
