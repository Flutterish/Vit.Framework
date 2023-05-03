﻿using SixLabors.ImageSharp.PixelFormats;
using System.Text;
using Vit.Framework.Graphics.Curses.Rendering;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Software.Textures;
using Vit.Framework.Memory;
using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.Curses.Queues;

public class Swapchain : DisposableObject, ISwapchain {
	Window window;
	public Swapchain ( Window window ) {
		this.window = window;
		backbuffer = new( window.PixelSize );
	}

	public void Recreate () {
		backbuffer.Dispose();
		backbuffer = new( window.PixelSize );
	}

	TargetImage backbuffer;
	public IFramebuffer? GetNextFrame ( out int frameIndex ) {
		if ( backbuffer.Size != window.Size )
			Recreate();

		frameIndex = 0;
		return backbuffer;
	}

	public void Present ( int frameIndex ) {
		var sb = new StringBuilder();
		sb.Append( "\u001B[1;1H" );
		sb.Append( "\u001B[?25l" );

		Rgba32 lastColor = default;

		var span = backbuffer.AsSpan2D();
		for ( int i = 0; i < backbuffer.Size.Height; i++ ) {
			foreach ( var px in span.GetRow( i ) ) {
				if ( lastColor != px ) {
					sb.Append( $"\u001B[48;2;{px.R};{px.G};{px.B}m" );
					lastColor = px;
				}
				sb.Append( ' ' );
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
		backbuffer.Dispose();
	}
}