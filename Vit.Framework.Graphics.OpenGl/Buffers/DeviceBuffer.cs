using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public class DeviceBuffer<T> : DisposableObject, IDeviceBuffer<T>, IGlBuffer where T : unmanaged {
	public int Handle { get; private set; }
	public DeviceBuffer ( uint size, BufferTarget type ) {
		Handle = GL.GenBuffer();
		GL.BindBuffer( type, Handle );
		GL.BufferStorage( type, (int)size, (nint)null, BufferStorageFlags.None );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
		deleteHandle();
	}

	[Conditional( "DEBUG" )]
	void deleteHandle () {
		Handle = 0;
	}
}
