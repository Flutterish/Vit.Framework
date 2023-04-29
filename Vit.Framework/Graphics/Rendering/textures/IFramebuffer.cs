using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Textures;

/// <summary>
/// A rendering target. Might have color, depth or stencil attachments and multisampling.
/// </summary>
public interface IFramebuffer : IDisposable {
	Size2<uint> Size { get; }
}
