using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Buffers;

public interface INativeBuffer<T> : IDisposable where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf<T>();

	public void Allocate ( ReadOnlySpan<T> data, BufferUsage usageHint ) {
		Allocate( data, data.Length, usageHint );
	}
	void Allocate ( ReadOnlySpan<T> data, int totalSize, BufferUsage usageHint );
	void Update ( ReadOnlySpan<T> data, int offset = 0 );
}
