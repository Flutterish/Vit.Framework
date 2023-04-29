namespace Vit.Framework.Graphics.Rendering.Shaders;

/// <summary>
/// A set of shader parts, used to create a graphics pipeline.
/// </summary>
public interface IShaderSet : IDisposable {
	IEnumerable<IShaderPart> Parts { get; }
}
