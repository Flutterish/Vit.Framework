﻿namespace Vit.Framework.Graphics.Rendering.Buffers;

public class StagedDeviceBuffer<T> : IDisposable where T : unmanaged {
	public readonly IStagingBuffer<T> StagingBuffer;
	public readonly IDeviceBuffer<T> DeviceBuffer;
	public StagedDeviceBuffer ( IRenderer renderer, uint size, BufferType type, BufferUsage stagingHint = BufferUsage.CpuWrite | BufferUsage.CopySource, BufferUsage deviceHint = BufferUsage.CopyDestination ) {
		StagingBuffer = renderer.CreateStagingBuffer<T>( size, stagingHint );
		DeviceBuffer = renderer.CreateDeviceBuffer<T>( size, type, deviceHint );
	}

	public void Upload ( ICommandBuffer commandBuffer, ReadOnlySpan<T> data ) {
		StagingBuffer.Upload( data );
		commandBuffer.CopyBuffer( StagingBuffer, DeviceBuffer, (uint)data.Length );
	}

	public void Dispose () {
		DeviceBuffer.Dispose();
		StagingBuffer.Dispose();
	}
}
