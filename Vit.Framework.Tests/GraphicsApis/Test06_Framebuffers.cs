using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Parsing.WaveFront;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test06_Framebuffers : GenericRenderThread {
	public Test06_Framebuffers ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
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
	StagedDeviceBuffer<Vertex> positions = null!;
	StagedDeviceBuffer<uint> indices = null!;
	StagedDeviceBuffer<Vertex> positions2 = null!;
	StagedDeviceBuffer<uint> indices2 = null!;
	IHostBuffer<Uniforms> uniformBuffer = null!;
	IHostBuffer<Uniforms> uniformBuffer2 = null!;
	Texture texture = null!;
	IUniformSet uniformSet = null!;
	IUniformSet uniformSet2 = null!;

	IDeviceTexture2D framebufferTexture = null!;
	ITexture2DView framebufferTextureView = null!;
	IDeviceTexture2D framebufferDepthTexture = null!;
	IFramebuffer framebuffer = null!;

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
		shaderSet = Renderer.CreateShaderSet( new[] { vertex, fragment }, VertexInputDescription.CreateSingle( vertex.ShaderInfo ) );

		positions = new( Renderer, BufferType.Vertex );
		indices = new( Renderer, BufferType.Index );
		positions2 = new( Renderer, BufferType.Vertex );
		indices2 = new( Renderer, BufferType.Index );
		uniformBuffer = Renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );
		uniformBuffer2 = Renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );
		using var image = Image.Load<Rgba32>( "./viking_room.png" );
		image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		texture = new( image );

		var model = SimpleObjModel.FromLines( File.ReadLines( "./viking_room.obj" ) );
		positions.Allocate( (uint)model.Vertices.Count, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		indices.Allocate( indexCount = (uint)model.Indices.Count, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		positions2.Allocate( 4, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		indices2.Allocate( 6, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		uniformBuffer.AllocateUniform( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		uniformBuffer2.AllocateUniform( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.GpuPerFrame );

		uniformSet = shaderSet.CreateUniformSet();
		uniformSet.SetUniformBuffer( uniformBuffer, binding: 0 );
		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			positions.Upload( commands, model.Vertices.Select( x => new Vertex {
				Position = x.Position.XYZ,
				UV = x.TextureCoordinates.XY
			} ).ToArray() );
			indices.Upload( commands, model.Indices.AsSpan() );

			texture.Update( commands );
			uniformSet.SetSampler( texture.View, texture.Sampler, binding: 1 );

			positions2.Upload( commands, new Vertex[] {
				new() { Position = (-0.9f, -0.9f, 0), UV = (0, 0) },
				new() { Position = (0.9f, -0.9f, 0), UV = (1, 0) },
				new() { Position = (-0.9f, 0.9f, 0), UV = (0, 1) },
				new() { Position = (0.9f, 0.9f, 0), UV = (1, 1) }
			} );
			indices2.Upload( commands, new uint[] {
				0, 1, 2,
				2, 1, 3
			} );
		}

		framebufferTexture = Renderer.CreateDeviceTexture( (256, 256), PixelFormat.Rgba8 );
		framebufferTextureView = framebufferTexture.CreateView();
		framebufferDepthTexture = Renderer.CreateDeviceTexture( (256, 256), PixelFormat.D24S8ui );
		framebuffer = Renderer.CreateFramebuffer( new[] { framebufferTexture }, framebufferDepthTexture );

		uniformSet2 = shaderSet.CreateUniformSet();
		uniformSet2.SetUniformBuffer( uniformBuffer2, binding: 0 );
		uniformSet2.SetSampler( framebufferTextureView, texture.Sampler, binding: 1 );
		return true;
	}

	DateTime start = DateTime.Now;
	protected override void Render ( IFramebuffer windowFramebuffer, ICommandBuffer commands ) {
		using ( commands.RenderTo( framebuffer, clearColor: new ColorHsv<Radians<float>, float> {
			H = ((float)(DateTime.Now - start).TotalSeconds).Radians(),
			S = 1,
			V = 1
		}.ToRgb(), clearDepth: 1 ) ) 
		{
			shaderSet.SetUniformSet( uniformSet );
			commands.SetShaders( shaderSet );
			commands.SetViewport( framebufferTexture.Size );
			commands.SetScissors( framebufferTexture.Size );

			commands.BindVertexBuffer( positions.DeviceBuffer );
			commands.BindIndexBuffer( indices.DeviceBuffer );
			uniformBuffer.UploadUniform( new Uniforms {
				ModelMatrix = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitX, -90f.Degrees() )
					* Matrix4<float>.CreateTranslation( 0, -0.3f, 0 )
					* Matrix4<float>.FromAxisAngle( Vector3<float>.UnitY, ((float)(DateTime.Now - start).TotalSeconds * 5).Degrees() )
					* Matrix4<float>.CreateTranslation( 0, 0, 1.2f )
					* Matrix4<float>.CreatePerspective( Window.Size.Width, Window.Size.Height, 0.01f, 100f )
					* Renderer.CreateUvCorrectionMatrix<float>()
			} );

			commands.SetTopology( Topology.Triangles );
			commands.SetDepthTest( new( CompareOperation.LessThan ), new() { WriteOnPass = true } );
			commands.DrawIndexed( indexCount );
		}

		using ( commands.RenderTo( windowFramebuffer, clearColor: new ColorHsv<Radians<float>, float> {
			H = ((float)(DateTime.Now - start).TotalSeconds * 2).Radians(),
			S = 1,
			V = 1
		}.ToRgb(), clearDepth: 1 ) ) {
			shaderSet.SetUniformSet( uniformSet2 );
			commands.SetShaders( shaderSet );
			commands.SetViewport( Swapchain.BackbufferSize );
			commands.SetScissors( Swapchain.BackbufferSize );

			commands.BindVertexBuffer( positions2.DeviceBuffer );
			commands.BindIndexBuffer( indices2.DeviceBuffer );
			uniformBuffer2.UploadUniform( new Uniforms {
				ModelMatrix = Renderer.CreateNdcCorrectionMatrix<float>()
			} );

			commands.SetTopology( Topology.Triangles );
			commands.SetDepthTest( new( CompareOperation.LessThan ), new() { WriteOnPass = true } );
			commands.DrawIndexed( 6 );
		}
	}

	protected override void Dispose () {
		indices.Dispose();
		positions.Dispose();
		indices2.Dispose();
		positions2.Dispose();
		uniformBuffer.Dispose();
		uniformBuffer2.Dispose();
		texture.Dispose();
		uniformSet.Dispose();
		uniformSet2.Dispose();

		framebuffer.Dispose();
		framebufferTextureView.Dispose();
		framebufferTexture.Dispose();
		framebufferDepthTexture.Dispose();

		shaderSet.Dispose();
		vertex.Dispose();
		fragment.Dispose();
	}
}
