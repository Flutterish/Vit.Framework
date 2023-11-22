﻿using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering.Buffers;

public class StagedDeviceBuffer<T> : DisposableObject where T : unmanaged {
	public readonly IStagingBuffer<T> StagingBuffer;
	public readonly IDeviceBuffer<T> DeviceBuffer;
	public StagedDeviceBuffer ( IRenderer renderer, uint size, BufferType type, BufferUsage stagingHint, BufferUsage deviceHint ) {
		StagingBuffer = renderer.CreateStagingBuffer<T>( size, stagingHint | BufferUsage.CpuWrite | BufferUsage.GpuRead );
		DeviceBuffer = renderer.CreateDeviceBuffer<T>( size, type, deviceHint | BufferUsage.GpuWrite );
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
