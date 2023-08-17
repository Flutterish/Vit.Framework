using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test03_Uniforms : GenericRenderThread {
	public Test03_Uniforms ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	IShaderPart vertex = null!;
	IShaderPart fragment = null!;
	IShaderSet shaderSet = null!;

	struct Vertex {
		public Point2<float> Position;
		public ColorRgb<float> Color;
	}

	struct Uniforms {
		public Matrix4<float> ModelMatrix;
	}

	StagedDeviceBuffer<Vertex> positions = null!;
	StagedDeviceBuffer<uint> indices = null!;
	IHostBuffer<Uniforms> uniformBuffer = null!;
	IUniformSet uniformSet = null!;
	protected override bool Initialize () {
		if ( !base.Initialize() )
			return false;

		vertex = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inPosition;
			layout(location = 1) in vec3 inColor;

			layout(location = 0) out vec3 outColor;

			layout(binding = 0) uniform Uniforms {
				mat4 model;
			} uniforms;

			void main () {
				outColor = inColor;
				gl_Position = uniforms.model * vec4(inPosition, 0, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
		fragment = Renderer.CompileShaderPart( new SpirvBytecode( @"#version 450
			layout(location = 0) in vec3 inColor;

			layout(location = 0) out vec4 outColor;

			void main () {
				outColor = vec4( inColor, 1 );
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );
		shaderSet = Renderer.CreateShaderSet( new[] { vertex, fragment }, VertexInputDescription.CreateSingle( vertex.ShaderInfo ) );

		positions = new( Renderer, BufferType.Vertex );
		indices = new( Renderer, BufferType.Index );
		uniformBuffer = Renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );

		positions.Allocate( 3, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		indices.Allocate( 3, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
		uniformBuffer.AllocateUniform( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.GpuPerFrame );

		uniformSet = shaderSet.CreateUniformSet();
		shaderSet.SetUniformSet( uniformSet );
		uniformSet.SetUniformBuffer( uniformBuffer, binding: 0 );
		using ( var commands = Renderer.CreateImmediateCommandBuffer() ) {
			positions.Upload( commands, new Vertex[] {
				new() { Position = new( 0, -0.5f ), Color = ColorRgb.Red },
				new() { Position = new( 0.7f, 0.3f ), Color = ColorRgb.Green },
				new() { Position = new( -0.5f, 0.7f ), Color = ColorRgb.Blue }
			} );
			indices.Upload( commands, new uint[] {
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
		}.ToRgb(), clearDepth: 1 );
		commands.SetShaders( shaderSet );
		commands.SetViewport( framebuffer.Size );
		commands.SetScissors( framebuffer.Size );

		commands.BindVertexBuffer( positions.DeviceBuffer );
		commands.BindIndexBuffer( indices.DeviceBuffer );
		uniformBuffer.UploadUniform( new Uniforms {
			ModelMatrix = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitY, ((float)(DateTime.Now - start).TotalSeconds * 50).Degrees() )
				* Matrix4<float>.CreateTranslation( 0, 0, 1.2f )
				* Renderer.CreateLeftHandCorrectionMatrix<float>()
				* Matrix4<float>.CreatePerspective( 1, 1, 0.01f, 100f )
		} );

		commands.SetTopology( Topology.Triangles );
		commands.DrawIndexed( 3 );
	}

	protected override void Dispose () {
		indices.Dispose();
		positions.Dispose();
		uniformBuffer.Dispose();
		uniformSet.Dispose();

		shaderSet.Dispose();
		vertex.Dispose();
		fragment.Dispose();
	}
}
