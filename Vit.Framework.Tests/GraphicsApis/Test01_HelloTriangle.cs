using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test01_HelloTriangle : GenericRenderThread {
	public Test01_HelloTriangle ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	IShaderPart vertex = null!;
	IShaderPart fragment = null!;
	IShaderSet shaderSet = null!;

	StagedDeviceBuffer<Point2<float>> positions = null!;
	StagedDeviceBuffer<uint> indices = null!;
	protected override bool Initialize () {
		if ( !base.Initialize() )
			return false;

		vertex = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inPosition;

			void main () {
				gl_Position = vec4(inPosition, 0, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
		fragment = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) out vec4 outColor;

			void main () {
				outColor = vec4( 0, 1, 0, 1 );
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );
		shaderSet = Renderer.CreateShaderSet( new[] { vertex, fragment }, VertexInputDescription.CreateSingle( vertex.ShaderInfo ) );

		positions = new( Renderer, BufferType.Vertex );
		indices = new( Renderer, BufferType.Index );

		positions.Allocate( 3, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		indices.Allocate( 3, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );

		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			positions.Upload( commands, new Point2<float>[] {
				new( 0, -0.5f ),
				new( 0.7f, 0.3f ),
				new( -0.5f, 0.7f )
			} );
			indices.Upload( commands, new uint[] {
				2, 1, 0
			} );
		}

		return true;
	}

	DateTime start = DateTime.Now;
	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer );
		commands.ClearColor( new ColorHsv<Radians<float>, float> {
			H = ((float)(DateTime.Now - start).TotalSeconds).Radians(),
			S = 1,
			V = 1
		}.ToRgb().ToSRgb() );
		commands.ClearDepth( 1 );

		commands.SetShaders( shaderSet );
		commands.SetViewport( Swapchain.BackbufferSize );
		commands.SetScissors( Swapchain.BackbufferSize );

		commands.BindVertexBuffer( positions.DeviceBuffer );
		commands.BindIndexBuffer( indices.DeviceBuffer );

		commands.SetTopology( Topology.Triangles );
		commands.DrawIndexed( 3 );
	}

	protected override void Dispose () {
		indices.Dispose();
		positions.Dispose();

		shaderSet.Dispose();
		vertex.Dispose();
		fragment.Dispose();
	}
}
