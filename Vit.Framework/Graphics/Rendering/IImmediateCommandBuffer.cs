namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// A <see cref="ICommandBuffer"/> which either executes commands immediately or when it is disposed.
/// </summary>
/// <remarks>
/// This should always be used within a <c>using</c> statement.
/// </remarks>
public interface IImmediateCommandBuffer : ICommandBuffer, IDisposable {

}
