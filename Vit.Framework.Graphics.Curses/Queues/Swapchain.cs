using System.Text;
using Vit.Framework.Graphics.Curses.Rendering;
using Vit.Framework.Graphics.Curses.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.Curses.Queues;

public class Swapchain : DisposableObject, ISwapchain {
	public Swapchain ( Window window ) {
		backbuffer = new() { Size = window.PixelSize };
	}

	public void Recreate () {
		throw new NotImplementedException();
	}

	Sprite backbuffer;
	public IFramebuffer? GetNextFrame ( out int frameIndex ) {
		frameIndex = 0;
		return backbuffer;
	}

	public void Present ( int frameIndex ) {
		var sb = new StringBuilder();
		sb.Append( "\u001B[1;1H" );
		sb.Append( "\u001B[?25l" );

		ColorRgba<byte> lastColor = default;

		var span = backbuffer.AsSpan();
		for ( int i = 0; i < backbuffer.Size.Height; i++ ) {
			foreach ( var px in span.GetRow( i ) ) {
				if ( lastColor != px.Background ) {
					sb.Append( $"\u001B[48;2;{px.Background.R};{px.Background.G};{px.Background.B}m" );
					lastColor = px.Background;
				}
				sb.Append( px.Symbol );
			}
			sb.Append( '\n' );
		}
		sb.Remove( sb.Length - 1, 1 );
		sb.Append( "\u001B[1;1H" );

		Console.Write( sb.ToString() );
	}

	CursesImmadiateCommandBuffer commandBuffer = new();
	public IImmediateCommandBuffer CreateImmediateCommandBufferForPresentation () {
		return commandBuffer;
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}
