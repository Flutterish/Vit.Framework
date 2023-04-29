using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public class Buffer<T> : DisposableObject, IDeviceBuffer<T>, IHostBuffer<T> where T : unmanaged {
	public readonly int Handle;
	public readonly BufferTarget Type;
	public Buffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );

		var offset = usageHint.HasFlag( BufferUsage.PerFrame ) ? 0 : usageHint.HasFlag( BufferUsage.Rarely ) ? 6 : 3;
		GL.BufferData( Type, (int)size, (nint)0, 
			usageHint.HasFlag( BufferUsage.CpuRead ) ? (BufferUsageHint.StreamRead + offset) :
			usageHint.HasFlag( BufferUsage.CpuWrite ) ? (BufferUsageHint.StreamDraw + offset) :
			(BufferUsageHint.StreamCopy + offset)
		);
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		GL.BindBuffer( Type, Handle );
		GL.BufferSubData( Type, (int)offset, data.Length, (nint)data.Data() );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
