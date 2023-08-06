using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test02_MultipleAttributes : GenericRenderThread {
	public Test02_MultipleAttributes ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	IShaderPart vertex = null!;
	IShaderPart fragment = null!;
	IShaderSet shaderSet = null!;

	struct Vertex {
		public Point2<float> Position;
		public ColorRgb<float> Color;
	}

	IDeviceBuffer<Vertex> positions = null!;
	IDeviceBuffer<uint> indices = null!;
	protected override bool Initialize () {
		if ( !base.Initialize() )
			return false;

		vertex = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inPosition;
			layout(location = 1) in vec3 inColor;

			layout(location = 0) out vec3 outColor;

			void main () {
				outColor = inColor;
				gl_Position = vec4(inPosition, 0, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
		fragment = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec3 inColor;

			layout(location = 0) out vec4 outColor;

			void main () {
				outColor = vec4( inColor, 1 );
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );
		shaderSet = Renderer.CreateShaderSet( new[] { vertex, fragment } );

		positions = Renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
		indices = Renderer.CreateDeviceBuffer<uint>( BufferType.Index );

		positions.Allocate( 3, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
		indices.Allocate( 3, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );

		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			commands.Upload( positions, new Vertex[] {
				new() { Position = new( 0, -0.5f ), Color = ColorRgb.Red },
				new() { Position = new( 0.7f, 0.3f ), Color = ColorRgb.Green },
				new() { Position = new( -0.5f, 0.7f ), Color = ColorRgb.Blue }
			} );
			commands.Upload( indices, new uint[] {
				2, 1, 0
			} );
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
