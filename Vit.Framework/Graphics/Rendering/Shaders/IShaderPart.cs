using Vit.Framework.Graphics.Rendering.Shaders.Reflections;

namespace Vit.Framework.Graphics.Rendering.Shaders;

/// <summary>
/// A programmable part of a graphics pipeline.
/// </summary>
public interface IShaderPart : IDisposable {
	ShaderPartType Type { get; }
	ShaderInfo ShaderInfo { get; }
}
