using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering.Buffers;

public class StagedDeviceBuffer<T> : DisposableObject where T : unmanaged {
	public readonly IStagingBuffer<T> StagingBuffer;
	public readonly IDeviceBuffer<T> DeviceBuffer;
	public StagedDeviceBuffer ( IRenderer renderer, BufferType type ) {
		StagingBuffer = renderer.CreateStagingBuffer<T>();
		DeviceBuffer = renderer.CreateDeviceBuffer<T>( type );
	}

	public void AllocateRaw ( uint size, BufferUsage stagingHint, BufferUsage deviceHint ) {
		StagingBuffer.AllocateRaw( size, stagingHint | BufferUsage.CpuWrite | BufferUsage.GpuRead );
		DeviceBuffer.AllocateRaw( size, deviceHint | BufferUsage.GpuWrite );
	}

	public void Allocate ( uint size, BufferUsage stagingHint, BufferUsage deviceHint ) {
		AllocateRaw( size * IBuffer<T>.Stride, stagingHint, deviceHint );
	}

	public void Upload ( ICommandBuffer commandBuffer, ReadOnlySpan<T> data ) {
		StagingBuffer.Upload( data );
		commandBuffer.CopyBuffer( StagingBuffer, DeviceBuffer, (uint)data.Length );
	}

	protected override void Dispose ( bool disposing ) {
		DeviceBuffer.Dispose();
		StagingBuffer.Dispose();
	}
}
