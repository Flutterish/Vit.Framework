using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Buffers;

public abstract class Buffer<T> where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf<T>();
}
