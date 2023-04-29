using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class DefaultFrameBuffer : IGlFramebuffer {
	public int Handle => 0;
	public Size2<uint> Size { get; set; }

	public void Dispose () { }
}
