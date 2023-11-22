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
	/// Stores uniform data. Generally used with <see cref="IHostBuffer"/>, given common updates. Each entry must be aligned to a renderer specific value, usually 256 bytes.
	/// </summary>
	Uniform,
	/// <summary>
	/// Stores arbitrary data which can be read from shaders. Generally used for unsized arrays or as a more tightly packed replacment for uniform buffers.
	/// </summary>
	/// <remarks>Can not be used with <see cref="BufferUsage.CpuRead"/>.</remarks>
	ReadonlyStorage,
	/// <summary>
	/// Stores arbitrary data which can be read and written to from shaders.
	/// </summary>
	/// <remarks>Can only be used with <see cref="IDeviceBuffer"/>.</remarks>
	//ReadWriteStorage
}
