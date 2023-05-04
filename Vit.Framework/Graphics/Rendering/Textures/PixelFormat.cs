namespace Vit.Framework.Graphics.Rendering.Textures;

/// <summary>
/// TBD - for now assume RGBA32 and assert against IsRGBA32
/// </summary>
public struct PixelFormat {
	public bool IsRGBA32 { get; internal set; }
	public static PixelFormat RGBA32 => new() { IsRGBA32 = true };
}