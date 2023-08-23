using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class FrameBuffer : DisposableObject, IGlFramebuffer {
	int IGlFramebuffer.Handle => Handle;
	public readonly int Handle;

	public FrameBuffer ( IEnumerable<ITexture2DView> attachments, IDeviceTexture2D? depthStencilAttachment = null ) {
		Handle = GL.GenFramebuffer();
		GL.BindFramebuffer( FramebufferTarget.ReadFramebuffer, Handle );

		if ( depthStencilAttachment != null ) {
			var handle = ((Texture2DStorage)depthStencilAttachment).Handle;
			if ( depthStencilAttachment.Format.Type == Graphics.Rendering.Textures.PixelType.DepthStencil ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, handle, 0 );
			}
			else if ( depthStencilAttachment.Format.Type == Graphics.Rendering.Textures.PixelType.Depth ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, handle, 0 );
			}
			else if ( depthStencilAttachment.Format.Type == Graphics.Rendering.Textures.PixelType.Stencil ) {
				GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.StencilAttachment, TextureTarget.Texture2D, handle, 0 );
			}
		}

		int colorIndex = 0;
		foreach ( var i in attachments ) {
			var view = (Texture2DView)i;
			var handle = view.Handle;
			GL.FramebufferTexture2D( FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0 + colorIndex, TextureTarget.Texture2D, handle, 0 );
			colorIndex++;
		}

		Debug.Assert( GL.CheckFramebufferStatus( FramebufferTarget.ReadFramebuffer ) == FramebufferErrorCode.FramebufferComplete );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteFramebuffer( Handle );
	}
}
