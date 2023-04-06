//using System.Runtime.InteropServices;
//using Vit.Framework.Graphics.Rendering.Buffers;

//namespace Vit.Framework.Graphics.OpenGl.Buffers;

//public class Buffer<T> : INativeBuffer<T> where T : unmanaged {
//	protected int Handle;
//	protected readonly BufferTarget Type;
//	public Buffer ( BufferTarget type ) {
//		Handle = GL.GenBuffer();
//		Type = type;
//	}

//	public void Allocate ( ReadOnlySpan<T> data, int totalSize, BufferUsage usageHint ) {
//		GL.BufferData<T>( Type, totalSize * INativeBuffer<T>.Stride, ref MemoryMarshal.GetReference( data ), DataTypes.Convert( usageHint ) );
//	}

//	public void Update ( ReadOnlySpan<T> data, int offset = 0 ) {
//		GL.BufferSubData<T>( Type, offset * INativeBuffer<T>.Stride, data.Length * INativeBuffer<T>.Stride, ref MemoryMarshal.GetReference( data ) );
//	}

//#if DEBUG
//	~Buffer () {
//		throw new Exception( "GPU Buffer was not disposed" );
//	}
//#endif

//	public void Dispose () {
//		GL.DeleteBuffer( Handle );
//		Handle = 0;
//#if DEBUG
//		GC.SuppressFinalize( this );
//#endif
//	}
//}
