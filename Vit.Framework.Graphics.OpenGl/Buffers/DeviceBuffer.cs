using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public class DeviceBuffer<T> : DisposableObject, IDeviceBuffer<T>, IGlBuffer where T : unmanaged {
	public uint Stride => IBuffer<T>.Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	public DeviceBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );
		GL.BufferStorage( Type, (int)size, (nint)null, BufferStorageFlags.None );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
