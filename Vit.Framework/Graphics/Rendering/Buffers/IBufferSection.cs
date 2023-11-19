using System.Runtime.InteropServices;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Rendering.Buffers;

public interface IBufferSection<TBuffer> where TBuffer : IBuffer {
	TBuffer Buffer { get; }
	uint ByteOffset { get; }
	uint ByteLength { get; }
}
public interface IBufferSection<TBuffer, T> : IBufferSection<TBuffer> where TBuffer : IBuffer where T : unmanaged {
	public uint ElementLength => ByteLength / SizeOfHelper<T>.Size;
}

public static class IBufferSectionExtensions {
	public static void Upload<T> ( this IBufferSection<IHostBuffer, T> section, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		section.Buffer.UploadRaw( MemoryMarshal.AsBytes( data ), offset * SizeOfHelper<T>.Size + section.ByteOffset );
	}
	public static void Upload<T> ( this IBufferSection<IHostBuffer> section, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		section.Buffer.UploadRaw( MemoryMarshal.AsBytes( data ), offset * SizeOfHelper<T>.Size + section.ByteOffset );
	}
	public static void Upload<T> ( this IBufferSection<IStagingBuffer, T> section, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		section.Buffer.UploadRaw( MemoryMarshal.AsBytes( data ), offset * SizeOfHelper<T>.Size + section.ByteOffset );
	}
	public static void Upload<T> ( this IBufferSection<IStagingBuffer> section, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		section.Buffer.UploadRaw( MemoryMarshal.AsBytes( data ), offset * SizeOfHelper<T>.Size + section.ByteOffset );
	}

	public static unsafe T* GetData<T> ( this IBufferSection<IStagingBuffer, T> section ) where T : unmanaged {
		return (T*)((byte*)section.Buffer.GetData() + section.ByteOffset);
	}
	public static unsafe T* GetData<T> ( this IBufferSection<IStagingBuffer> section ) where T : unmanaged {
		return (T*)((byte*)section.Buffer.GetData() + section.ByteOffset);
	}
	public static unsafe T* Map<T> ( this IBufferSection<IHostBuffer, T> section ) where T : unmanaged {
		return (T*)((byte*)section.Buffer.Map() + section.ByteOffset);
	}
	public static unsafe T* Map<T> ( this IBufferSection<IHostBuffer> section ) where T : unmanaged {
		return (T*)((byte*)section.Buffer.Map() + section.ByteOffset);
	}
	public static void Unmap ( this IBufferSection<IHostBuffer> section ) {
		section.Buffer.Unmap();
	}
}