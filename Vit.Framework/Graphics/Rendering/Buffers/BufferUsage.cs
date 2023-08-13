namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// Specifies how a buffer will be used - this allows for optimized memory allocation.
/// </summary>
[Flags]
public enum BufferUsage {
	None = 0,
	CpuRead = 1,
	CpuWrite = 2,
	GpuRead = 4,
	GpuWrite = 8,

	GpuRarely = 16,
	GpuPerFrame = 16 + 32,

	CpuRarely = 64,
	CpuPerFrame = 64 + 128
}
