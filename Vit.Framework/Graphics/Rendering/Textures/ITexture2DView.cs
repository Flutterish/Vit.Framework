namespace Vit.Framework.Graphics.Rendering.Textures;

public interface ITexture2DView : IDisposable {
	IDeviceTexture2D Source { get; }
}
