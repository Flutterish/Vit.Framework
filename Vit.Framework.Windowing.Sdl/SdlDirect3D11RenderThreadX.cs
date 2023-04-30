using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Queues;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Threading;
using Vortice.Direct3D;

namespace Vit.Framework.Windowing.Sdl;

public class SdlDirect3D11RenderThreadX : AppThread {
	SdlWindow window;
	public SdlDirect3D11RenderThreadX ( SdlWindow window, string name ) : base( name ) {
		this.window = window;
	}

	IHostBuffer<float> buffer = null!;
	IShaderPart vs = null!;
	IShaderPart fs = null!;
	IShaderSet shaders = null!;

	Swapchain swapChain = null!;
	Direct3D11Renderer renderer = null!;
	protected override void Initialize () {
		(swapChain, renderer) = ((Swapchain, Direct3D11Renderer))window.CreateSwapchain( new Direct3D11Api( new[] { RenderingCapabilities.DrawToWindow } ), new() {} );

		var shader = @"/* vertex attributes go here to input to the vertex shader */
		struct vs_in {
			float3 position_local : POS;			
			float3 color : COL;
		};

		/* outputs from vertex shader go here. can be interpolated to pixel shader */
		struct vs_out {
			float4 position_clip : SV_POSITION; // required output of VS
			float3 color : COL;
		};

		vs_out vs_main(vs_in input) {
			vs_out output = (vs_out)0; // zero the memory first
			output.position_clip = float4(input.position_local, 1.0);
			output.color = input.color;
			return output;
		}

		float4 ps_main(vs_out input) : SV_TARGET {
			return float4( input.color.r, input.color.g, input.color.b, 1.0 ); // must return an RGBA colour
		}";
		var vertSpirv = new SpirvBytecode( shader, ShaderLanguage.HLSL, ShaderPartType.Vertex );
		var fragSpirv = new SpirvBytecode( shader, ShaderLanguage.HLSL, ShaderPartType.Fragment );

		vs = renderer.CompileShaderPart( vertSpirv );
		fs = renderer.CompileShaderPart( fragSpirv );
		shaders = renderer.CreateShaderSet( new[] { vs, fs } );

		float[] vertices = {
		   0.0f,  0.5f,  0.0f, 1, 0, 0, // point at top
		   0.5f, -0.5f,  0.0f, 0, 1, 0, // point at bottom-right
		  -0.5f, -0.5f,  0.0f, 0, 0, 1  // point at bottom-left
		};

		buffer = renderer.CreateHostBuffer<float>( BufferType.Vertex );
		buffer.Allocate( 18, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.PerFrame );
		buffer.Upload( vertices );

		var rng = new Random();
		clearColor = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 );
	}

	ColorRgba<float> clearColor;
	protected override void Loop () {
		if ( swapChain.GetNextFrame( out var index ) is not TargetView target ) {
			return;
		}

		using ( var commands = swapChain.CreateImmediateCommandBufferForPresentation() ) {
			using var _ = commands.RenderTo( target, clearColor );
			commands.SetViewport( window.PixelSize );

			renderer.Context.IASetPrimitiveTopology( PrimitiveTopology.TriangleList );
			renderer.Context.IASetInputLayout( ((ShaderSet)shaders).Layout! );
			renderer.Context.IASetVertexBuffer( 0, ((Buffer<float>)buffer).Handle!, 6 * sizeof( float ) );

			renderer.Context.VSSetShader( ((VertexShader)vs).Handle );
			renderer.Context.PSSetShader( ((PixelShader)fs).Handle );

			renderer.Context.Draw( 3, 0 );
		}

		swapChain.Present( index );
		Sleep(1);
	}

	protected override void Dispose ( bool disposing ) {
		renderer.Dispose();
		buffer.Dispose();
		shaders.Dispose();
		vs.Dispose();
		fs.Dispose();
		swapChain.Dispose();
	}
}
