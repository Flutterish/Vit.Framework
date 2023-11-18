using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Rendering.Pooling;

/// <summary>
/// A stack allocator for single-use buffer sections from larger buffers. The rented sections are only valid for one frame.
/// </summary>
public class SingleUseBufferSectionStack {
	public readonly uint BufferLength;
	IRenderer Renderer = null!;
	public SingleUseBufferSectionStack ( uint bufferLength ) {
		BufferLength = bufferLength;

		stagingBuffers = new( bufferLength, length => {
			var buffer = Renderer.CreateStagingBuffer<byte>();
			buffer.AllocateRaw( length, BufferUsage.GpuRead | BufferUsage.CpuWrite );
			return buffer;
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
				var buffer = stack.Renderer.CreateDeviceBuffer<byte>( type );
				buffer.AllocateRaw( length, BufferUsage.GpuRead | BufferUsage.GpuWrite );
				return buffer;
			} );
			HostBuffers = new( stack.BufferLength, length => {
				var buffer = stack.Renderer.CreateHostBuffer<byte>( type );
				buffer.AllocateRaw( length, BufferUsage.GpuRead | BufferUsage.CpuWrite );
				return buffer;
			} );
		}

		public AllocationStack<IDeviceBuffer> DeviceBuffers;
		public AllocationStack<IHostBuffer> HostBuffers;
	}

	public struct Allocation<T> where T : IBuffer {
		public T Buffer;
		public uint Offset;
		public uint Length;
	}
	public Allocation<IStagingBuffer> AllocateStagingBuffer<T> ( uint length ) where T : unmanaged {
		return stagingBuffers.Allocate( length * SizeOfHelper<T>.Size );
	}
	public Allocation<IHostBuffer> AllocateHostBuffer<T> ( uint length, BufferType type ) where T : unmanaged {
		if ( !buffersByType.TryGetValue( type, out var buffers ) ) {
			buffersByType.Add( type, buffers = new( this, type ) );
		}

		return buffers.HostBuffers.Allocate( length * SizeOfHelper<T>.Size );
	}
	public Allocation<IDeviceBuffer> AllocateDeviceBuffer<T> ( uint length, BufferType type ) where T : unmanaged {
		if ( !buffersByType.TryGetValue( type, out var buffers ) ) {
			buffersByType.Add( type, buffers = new( this, type ) );
		}

		return buffers.DeviceBuffers.Allocate( length * SizeOfHelper<T>.Size );
	}

	class AllocationStack<T> where T : IBuffer {
		uint bufferLength;
		List<(T buffer, uint remainingLength)> buffers = new();
		Func<uint, T> creator;
		public AllocationStack ( uint bufferLength, Func<uint, T> creator ) {
			this.bufferLength = bufferLength;
			this.creator = creator;
		}

		public Allocation<T> Allocate ( uint length ) {
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
}

public static class SingleUseBufferSectionStackExtensions {
	public static void Upload<T> ( this SingleUseBufferSectionStack.Allocation<IHostBuffer> allocation, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		allocation.Buffer.UploadRaw( MemoryMarshal.AsBytes( data ), offset * SizeOfHelper<T>.Size + allocation.Offset );
	}
}