using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public interface IGlFramebuffer : IFramebuffer {
	int Handle { get; }
}
