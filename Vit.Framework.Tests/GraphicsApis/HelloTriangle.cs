using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class HelloTriangle : GenericRenderThread {
	public HelloTriangle ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	IShaderPart vertex = null!;
	IShaderPart fragment = null!;
	IShaderSet shaderSet = null!;

	IDeviceBuffer<Point2<float>> positions = null!;
	IDeviceBuffer<uint> indices = null!;
	protected override void Initialize () {
		base.Initialize();

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
		shaderSet = Renderer.CreateShaderSet( new[] { vertex, fragment } );

		positions = Renderer.CreateDeviceBuffer<Point2<float>>( BufferType.Vertex );
		indices = Renderer.CreateDeviceBuffer<uint>( BufferType.Index );

		positions.Allocate( 3, BufferUsage.GpuRead | BufferUsage.PerFrame );
		indices.Allocate( 3, BufferUsage.GpuRead | BufferUsage.PerFrame );

		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			commands.Upload( positions, new Point2<float>[] {
				new( 0, -0.5f ),
				new( 0.5f, 0.5f ),
				new( -0.5f, 0.5f )
			} );
			commands.Upload( indices, new uint[] {
				0, 1, 2
			} );
		}
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer, clearColor: ColorRgba.HotPink, clearDepth: 1 );
		commands.SetShaders( shaderSet );
		commands.SetViewport( framebuffer.Size );
		commands.SetScissors( framebuffer.Size );

		commands.BindVertexBuffer( positions );
		commands.BindIndexBuffer( indices );

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
