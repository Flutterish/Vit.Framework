using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Buffers;

public interface INativeBuffer<T> where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf<T>();
}
