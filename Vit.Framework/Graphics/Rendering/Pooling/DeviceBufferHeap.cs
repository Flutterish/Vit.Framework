using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;
using Vit.Framework.Memory.Allocation;

namespace Vit.Framework.Graphics.Rendering.Pooling;

/// <summary>
/// A heap allocator for <see cref="IDeviceBuffer"/> sections from larger buffers.
/// </summary>
public class DeviceBufferHeap : IDisposable {
	IRenderer renderer = null!;
	uint bufferLength;
	uint expectedSize;
	public DeviceBufferHeap ( uint bufferLength, uint expectedAllocationSize ) {
		this.bufferLength = bufferLength;
		expectedSize = expectedAllocationSize;
	}

	public void Initialize ( IRenderer renderer ) {
		this.renderer = renderer;
	}

	Dictionary<BufferType, List<(IDeviceBuffer buffer, DecoupledHeapAllocator allocator)>> buffersByType = new();
	public struct Allocation<T> : IBufferSection<IDeviceBuffer, T>, IDisposable where T : unmanaged {
		public IDeviceBuffer Buffer;
		public uint Offset;
		public uint Length;

		public DecoupledHeapAllocator Heap;
		public BufferType Type;

		readonly IDeviceBuffer IBufferSection<IDeviceBuffer>.Buffer => Buffer;
		readonly uint IBufferSection<IDeviceBuffer>.ByteOffset => Offset;
		readonly uint IBufferSection<IDeviceBuffer>.ByteLength => Length;

		public void Dispose () {
			Heap.Free( Offset );
			removeHeap();
		}

		[Conditional("DEBUG")]
		void removeHeap () {
			Heap = null!;
		}
	}

	public Allocation<T> Allocate<T> ( BufferType type, uint length ) where T : unmanaged {
		length *= SizeOfHelper<T>.Size;
		if ( !buffersByType.TryGetValue( type, out var buffers ) ) {
			buffersByType.Add( type, buffers = new() );
		}

		for ( int i = 0; i < buffers.Count; i++ ) {
			if ( length > (bufferLength << i) )
				continue;

			var buffer = buffers[i];
			var allocation = buffer.allocator.Allocate( length );
			if ( allocation.Bytes == 0 )
				continue;

			return new() {
				Buffer = buffer.buffer,
				Offset = (uint)allocation.Pointer,
				Length = (uint)allocation.Bytes,
				Heap = buffer.allocator,
				Type = type
			};
		}

		while ( length > (bufferLength << buffers.Count) ) {
			var nextBufferLength = bufferLength << buffers.Count;
			var nextBuffer = renderer.CreateDeviceBuffer<byte>( type );
			nextBuffer.Allocate( nextBufferLength, BufferUsage.GpuRead | BufferUsage.GpuWrite | BufferUsage.GpuPerFrame );
			buffers.Add(( nextBuffer, new DecoupledHeapAllocator( nextBufferLength, expectedSize << buffers.Count ) ));
		}

		var newBufferLength = bufferLength << buffers.Count;
		var newBuffer = renderer.CreateDeviceBuffer<byte>( type );
		newBuffer.Allocate( newBufferLength, BufferUsage.GpuRead | BufferUsage.GpuWrite | BufferUsage.GpuPerFrame );
		var allocator = new DecoupledHeapAllocator( newBufferLength, expectedSize << buffers.Count );
		buffers.Add( (newBuffer, allocator) );
		var newAllocation = allocator.Allocate( length );
		return new() {
			Buffer = newBuffer,
			Offset = (uint)newAllocation.Pointer,
			Length = (uint)newAllocation.Bytes,
			Heap = allocator,
			Type = type
		};
	}

	public Allocation<T> Reallocate<T> ( Allocation<T> allocation, uint size, out bool moved ) where T : unmanaged {
		var length = size * SizeOfHelper<T>.Size;
		var reallocation = allocation.Heap.Reallocate( allocation.Offset, length, out moved );
		if ( reallocation.Bytes != 0 ) {
			return allocation with {
				Offset = (uint)reallocation.Pointer,
				Length = (uint)reallocation.Bytes
			};
		}

		moved = true;
		allocation.Dispose();
		return Allocate<T>( allocation.Type, size );
	}

	public void Dispose () {
		foreach ( var i in buffersByType.Values ) {
			foreach ( var (buffer, _) in i ) {
				buffer.Dispose();
			}
		}
	}
}
