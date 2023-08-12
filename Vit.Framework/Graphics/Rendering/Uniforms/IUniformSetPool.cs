namespace Vit.Framework.Graphics.Rendering.Uniforms;

/// <summary>
/// A pool of <see cref="IUniformSet"/>s.
/// </summary>
public interface IUniformSetPool : IDisposable {
	IUniformSet Rent ();
	void Free ( IUniformSet set );
}
