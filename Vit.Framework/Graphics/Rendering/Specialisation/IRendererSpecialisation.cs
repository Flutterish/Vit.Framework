namespace Vit.Framework.Graphics.Rendering.Specialisation;

/// <summary>
/// A marker interface for generic constraints.
/// Renderer specialisation is based on the fact that value types are subject to special optimisations in generic code.
/// When generic code over a value type is emmited, const members are inlined and type checks are performed at JIT-compile-time, allowing for 0-cost specialised code.
/// Please note that if such a value type must not be a generic over a reference type, as this disables such optimisations.
/// This can be used standalone when using a specific renderer whose return signature is a specific type, 
/// or when using multiple possible renderers, a top-level generic function can be generated with reflections.
/// </summary>
public interface IRendererSpecialisation {

}
