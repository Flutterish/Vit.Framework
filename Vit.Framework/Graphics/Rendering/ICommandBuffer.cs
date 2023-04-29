using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// An interface with which you can send commands to the GPU.
/// </summary>
public interface ICommandBuffer {
	/// <summary>
	/// Starts rendering to a frame buffer.
	/// </summary>
	/// <returns>An action which will finish rendering to the framebuffer.</returns>
	DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, Color4<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null );
	// TODO instead of the action, perhaps we need a stack of state-related stuff

	/// <summary>
	/// Uploads data to a buffer.
	/// </summary>
	/// <param name="buffer">The device buffer to upload to.</param>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged;

	void SetShaders ( IShaderSet? shaders );

	void SetTopology ( Topology topology );

	void SetViewport ( AxisAlignedBox2<float> viewport );
	void SetScissors ( AxisAlignedBox2<uint> scissors );

	void BindVertexBuffer ( IBuffer buffer );
	void BindIndexBuffer ( IBuffer buffer );

	void DrawIndexed ( uint vertexCount, uint offset = 0 );
}
