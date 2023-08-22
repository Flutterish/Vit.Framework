using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class FrameBuffer : DisposableObject, IGlFramebuffer {
	int IGlFramebuffer.Handle => Handle;
	public readonly int Handle;
	public Size2<uint> Size { get; }

	public FrameBuffer ( IEnumerable<ITexture2DView> attachments ) {
		Handle = GL.GenFramebuffer();
		GL.BindFramebuffer( FramebufferTarget.ReadFramebuffer, Handle );

		int colorIndex = 0;
		foreach ( var i in attachments ) {
			var view = (Texture2DView)i;
			Size = view.Source.Size;
			if ( i.Source.Format.Type == Graphics.Rendering.Textures.PixelType.Color ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0 + colorIndex, TextureTarget.Texture2D, view.Handle, 0 );
				colorIndex++;
			}
			else if ( i.Source.Format.Type == Graphics.Rendering.Textures.PixelType.DepthStencil ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, view.Handle, 0 );
			}
			else if ( i.Source.Format.Type == Graphics.Rendering.Textures.PixelType.Depth ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, view.Handle, 0 );
			}
			else if ( i.Source.Format.Type == Graphics.Rendering.Textures.PixelType.Stencil ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.StencilAttachment, TextureTarget.Texture2D, view.Handle, 0 );
			}
		}

		Debug.Assert( GL.CheckFramebufferStatus( FramebufferTarget.ReadFramebuffer ) == FramebufferErrorCode.FramebufferComplete );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteFramebuffer( Handle );
	}
}
