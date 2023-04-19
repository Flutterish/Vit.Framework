namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// A cache/queue of commands, to be executed later. This allows for multi-thread rendering.
/// </summary>
/// <remarks>
/// This might require emulation on certain backends (such as OpenGL), if you do not require this functionality you should use <see cref="IImmediateCommandBuffer"/>.
/// </remarks>
public interface ICommandCache : ICommandBuffer {

}
