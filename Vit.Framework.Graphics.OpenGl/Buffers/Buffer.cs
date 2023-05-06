using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public interface IGlBuffer : IGlObject {
	int Stride { get; }
}

public class Buffer<T> : DisposableObject, IGlBuffer, IDeviceBuffer<T>, IHostBuffer<T> where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default(T) );
	int IGlBuffer.Stride => Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	public Buffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );

		var offset = usageHint.HasFlag( BufferUsage.GpuPerFrame ) ? 0 : usageHint.HasFlag( BufferUsage.GpuRarely ) ? 6 : 3;
		GL.BufferData( Type, (int)size * Stride, (nint)null, 
			usageHint.HasFlag( BufferUsage.CpuRead ) ? (BufferUsageHint.StreamRead + offset) :
			usageHint.HasFlag( BufferUsage.CpuWrite ) ? (BufferUsageHint.StreamDraw + offset) :
			(BufferUsageHint.StreamCopy + offset)
		);
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		GL.BindBuffer( Type, Handle );
		GL.BufferSubData( Type, (int)offset, data.Length * Stride, (nint)data.Data() );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
