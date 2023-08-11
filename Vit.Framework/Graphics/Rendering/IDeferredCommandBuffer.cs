using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// A cache/queue of commands, to be executed later. This allows for baking renders or multi-thread rendering.
/// </summary>
/// <remarks>
/// This might require emulation on certain backends (such as OpenGL), if you do not require this functionality you should use <see cref="IImmediateCommandBuffer"/>.
/// </remarks>
public interface IDeferredCommandBuffer : ICommandBuffer {
	/// <summary>
	/// Clears the cache. Must be called before <see cref="Begin"/>.
	/// </summary>
	void Reset ();

	/// <summary>
	/// Begins recording to the cache.
	/// </summary>
	DisposeAction<IDeferredCommandBuffer> Begin ();
}