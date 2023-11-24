using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Rendering.Pooling;

/// <summary>
/// A stack allocator for single-use buffer sections from larger buffers. The rented sections are only valid for one frame.
/// </summary>
public class SingleUseBufferSectionStack : IDisposable {
	public readonly uint BufferLength;
	IRenderer Renderer = null!;
	public SingleUseBufferSectionStack ( uint bufferLength ) {
		BufferLength = bufferLength;

		stagingBuffers = new( bufferLength, length => {
			return Renderer.CreateStagingBuffer<byte>( length, BufferUsage.GpuRead | BufferUsage.CpuWrite );
		} );
	}

	public void Initialize ( IRenderer renderer ) {
		Renderer = renderer;
	}

	Dictionary<BufferType, Buffers> buffersByType = new();
	AllocationStack<IStagingBuffer> stagingBuffers;
	struct Buffers {
		public Buffers ( SingleUseBufferSectionStack stack, BufferType type ) {
			DeviceBuffers = new( stack.BufferLength, length => {
				return stack.Renderer.CreateDeviceBuffer<byte>( length, type, BufferUsage.GpuRead | BufferUsage.GpuWrite );
			} );
			HostBuffers = new( stack.BufferLength, length => {
				return stack.Renderer.CreateHostBuffer<byte>( length, type, BufferUsage.GpuRead | BufferUsage.CpuWrite );
			} );
		}

		public AllocationStack<IDeviceBuffer> DeviceBuffers;
		public AllocationStack<IHostBuffer> HostBuffers;
	}

	public struct Allocation<TBuffer, T> : IBufferSection<TBuffer, T> where TBuffer : IBuffer where T : unmanaged {
		public TBuffer Buffer;
		public uint Offset;
		public uint Length;

		readonly TBuffer IBufferSection<TBuffer>.Buffer => Buffer;
		readonly uint IBufferSection<TBuffer>.ByteOffset => Offset;
		readonly uint IBufferSection<TBuffer>.ByteLength => Length;
	}
	public Allocation<IStagingBuffer, T> AllocateStagingBuffer<T> ( uint length ) where T : unmanaged {
		return stagingBuffers.Allocate<T>( length );
	}
	public Allocation<IHostBuffer, T> AllocateHostBuffer<T> ( uint length, BufferType type ) where T : unmanaged {
		if ( !buffersByType.TryGetValue( type, out var buffers ) ) {
			buffersByType.Add( type, buffers = new( this, type ) );
		}

		return buffers.HostBuffers.Allocate<T>( length );
	}
	public Allocation<IDeviceBuffer, T> AllocateDeviceBuffer<T> ( uint length, BufferType type ) where T : unmanaged {
		if ( !buffersByType.TryGetValue( type, out var buffers ) ) {
			buffersByType.Add( type, buffers = new( this, type ) );
		}

		return buffers.DeviceBuffers.Allocate<T>( length );
	}

	class AllocationStack<TBuffer> : IDisposable where TBuffer : IBuffer {
		uint bufferLength;
		List<(TBuffer buffer, uint remainingLength)> buffers = new();
		Func<uint, TBuffer> creator;
		public AllocationStack ( uint bufferLength, Func<uint, TBuffer> creator ) {
			this.bufferLength = bufferLength;
			this.creator = creator;
		}

		public Allocation<TBuffer, T> Allocate<T> ( uint length ) where T : unmanaged {
			length *= SizeOfHelper<T>.Size;

			for ( int i = 0; i < buffers.Count; i++ ) {
				var buffer = buffers[i];
				if ( buffer.remainingLength >= length ) {
					buffer.remainingLength -= length;
					buffers[i] = buffer;
					return new() {
						Buffer = buffer.buffer,
						Length = length,
						Offset = buffer.remainingLength
					};
				}
			}

			while ( length > (bufferLength << buffers.Count) ) {
				var nextBufferLength = bufferLength << buffers.Count;
				var nextBuffer = creator( nextBufferLength );
				buffers.Add( (nextBuffer, nextBufferLength) );
			}

			var newBufferLength = bufferLength << buffers.Count;
			var newBuffer = creator( newBufferLength );
			buffers.Add( (newBuffer, newBufferLength - length) );
			return new() {
				Buffer = newBuffer,
				Length = length,
				Offset = newBufferLength - length
			};
		}

		public void EndFrame () {
			for ( int i = 0; i < buffers.Count; i++ ) {
				buffers[i] = buffers[i] with { remainingLength = bufferLength << i };
			}
		}

		public void Dispose () {
			foreach ( var (i, _) in buffers ) {
				i.Dispose();
			}
			buffers.Clear();
		}
	}

	/// <summary>
	/// Marks all buffers as free.
	/// </summary>
	public void EndFrame () {
		stagingBuffers.EndFrame();
		foreach ( var i in buffersByType.Values ) {
			i.HostBuffers.EndFrame();
			i.DeviceBuffers.EndFrame();
		}
	}

	public void Dispose () {
		stagingBuffers.Dispose();
		foreach ( var i in buffersByType.Values ) {
			i.HostBuffers.Dispose();
			i.DeviceBuffers.Dispose();
		}
	}
}
