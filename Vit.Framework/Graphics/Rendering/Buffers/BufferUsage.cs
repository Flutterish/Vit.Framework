namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// Specifies how a buffer will be used - this allows for optimized memory allocation.
/// </summary>
[Flags]
public enum BufferUsage {
	CpuRead = 1,
	CpuWrite = 2,
	GpuRead = 4,
	GpuWrite = 8,

	Rarely = 16,
	PerFrame = 16 + 32
}
