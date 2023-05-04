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
	DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null );
	// TODO instead of the action, perhaps we need a stack of state-related stuff

	/// <summary>
	/// Uploads data to a buffer.
	/// </summary>
	/// <param name="buffer">The device buffer to upload to.</param>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged;

	/// <summary>
	/// Uploads data to a texture.
	/// </summary>
	/// <typeparam name="TPixel">The type of pixel to upload</typeparam>
	/// <param name="texture">The texture to upload to.</param>
	/// <param name="data">The image data to upload.</param>
	void UploadTextureData<TPixel> ( ITexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged;

	/// <summary>
	/// Sets the shaders for the rendering pipeline.
	/// </summary>
	void SetShaders ( IShaderSet? shaders );

	/// <summary>
	/// Sets the topology that will be used when drawing elements.
	/// </summary>
	void SetTopology ( Topology topology );

	/// <summary>
	/// Maps NDC to frame buffer coordinates.
	/// </summary>
	void SetViewport ( AxisAlignedBox2<uint> viewport );
	
	/// <summary>
	/// Sets which region of the framebuffer pixels can be rendered to.
	/// </summary>
	void SetScissors ( AxisAlignedBox2<uint> scissors );

	/// <summary>
	/// Maps a packed vertex buffer to a all locations in the current shader set.
	/// </summary>
	void BindVertexBuffer ( IBuffer buffer ); // TODO allow multiple buffers with shader set and mesh descriptors

	/// <summary>
	/// Sets the index buffer to use when drawing indexed elements.
	/// </summary>
	void BindIndexBuffer ( IBuffer buffer );

	/// <summary>
	/// Draws using the index buffer.
	/// </summary>
	/// <param name="vertexCount">Amount of vertices to draw (*not* elements).</param>
	/// <param name="offset">Offset from the start of the index buffer.</param>
	void DrawIndexed ( uint vertexCount, uint offset = 0 );
}
