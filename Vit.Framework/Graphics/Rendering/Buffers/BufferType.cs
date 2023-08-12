namespace Vit.Framework.Graphics.Rendering.Buffers;

public enum BufferType {
	/// <summary>
	/// Stores per-vertex data. Generally used with <see cref="IDeviceBuffer"/>.
	/// </summary>
	Vertex,
	/// <summary>
	/// Stores indices. Generally used with <see cref="IDeviceBuffer"/>.
	/// </summary>
	Index,
	/// <summary>
	/// Stores uniform data. Generally used with <see cref="IHostBuffer"/>, given common updates.
	/// </summary>
	Uniform
}
