namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// Specifies how a buffer will be used - this allows for optimized memory allocation.
/// </summary>
[Flags]
public enum BufferUsage {
	Default = 0,
	CpuRead = 1,
	CpuWrite = 2,

	CopySource = 4,
	CopyDestination = 8
}
