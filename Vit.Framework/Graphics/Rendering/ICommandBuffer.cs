using System.Runtime.CompilerServices;
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

	IShaderSet ShaderSet { get; }
	/// <summary>
	/// Sets the shaders for the rendering pipeline.
	/// </summary>
	void SetShaders ( IShaderSet shaders );

	Topology Topology { get; }
	/// <summary>
	/// Sets the topology that will be used when drawing elements.
	/// </summary>
	void SetTopology ( Topology topology );

	AxisAlignedBox2<uint> Viewport { get; }
	/// <summary>
	/// Maps NDC to frame buffer coordinates.
	/// </summary>
	void SetViewport ( AxisAlignedBox2<uint> viewport );

	AxisAlignedBox2<uint> Scissors { get; }
	/// <summary>
	/// Sets which region of the framebuffer pixels can be rendered to.
	/// </summary>
	void SetScissors ( AxisAlignedBox2<uint> scissors );

	BufferTest DepthTest { get; }
	/// <summary>
	/// Sets the depth testing behaviour.
	/// </summary>
	void SetDepthTest ( BufferTest test );

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

public static class ICommandBufferExtensions {
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, IShaderSet shaderSet)> PushShaderSet ( this ICommandBuffer self, IShaderSet shaderSet ) {
		var previous = self.ShaderSet;
		self.SetShaders( shaderSet );
		return new( (self, previous), static data => {
			data.buffer.SetShaders( data.shaderSet );
		} );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, Topology topology)> PushTopology ( this ICommandBuffer self, Topology topology ) {
		var previous = self.Topology;
		self.SetTopology( topology );
		return new( (self, previous), static data => {
			data.buffer.SetTopology( data.topology );
		} );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, AxisAlignedBox2<uint> viewport)> PushViewport ( this ICommandBuffer self, AxisAlignedBox2<uint> viewport ) {
		var previous = self.Viewport;
		self.SetViewport( viewport );
		return new( (self, previous), static data => {
			data.buffer.SetViewport( data.viewport );
		} );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, AxisAlignedBox2<uint> scissors)> PushScissors ( this ICommandBuffer self, AxisAlignedBox2<uint> scissors ) {
		var previous = self.Scissors;
		self.SetScissors( scissors );
		return new( (self, previous), static data => {
			data.buffer.SetScissors( data.scissors );
		} );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, BufferTest depthTest)> PushDepthTest ( this ICommandBuffer self, BufferTest depthTest ) {
		var previous = self.DepthTest;
		self.SetDepthTest( depthTest );
		return new( (self, previous), static data => {
			data.buffer.SetDepthTest( data.depthTest );
		} );
	}
}

/// <summary>
/// Basic implementation of state-tracking for an <see cref="ICommandBuffer"/>.
/// </summary>
public abstract class BasicCommandBuffer<TFramebuffer, TTexture, TShaderSet> : ICommandBuffer
	where TFramebuffer : class, IFramebuffer
	where TTexture : class, ITexture
	where TShaderSet : class, IShaderSet 
{
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		Invalidations |= PipelineInvalidations.Framebuffer;
		return RenderTo( (TFramebuffer)framebuffer, clearColor ?? ColorRgba.Black, clearDepth ?? 0, clearStencil ?? 0 );
	}
	protected abstract DisposeAction<ICommandBuffer> RenderTo ( TFramebuffer framebuffer, ColorRgba<float> clearColor, float clearDepth, uint clearStencil );

	public abstract void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged;

	public void UploadTextureData<TPixel> ( ITexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		UploadTextureData( (TTexture)texture, data );
	}
	protected abstract void UploadTextureData<TPixel> ( TTexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged;

	protected PipelineInvalidations Invalidations { get; private set; } = PipelineInvalidations.DepthTest;

	IShaderSet ICommandBuffer.ShaderSet => ShaderSet;
	protected TShaderSet ShaderSet { get; private set; } = null!;
	public void SetShaders ( IShaderSet shaders ) {
		if ( shaders == ShaderSet )
			return;

		ShaderSet = (TShaderSet)shaders;
		Invalidations |= PipelineInvalidations.Shaders;
	}

	public Topology Topology { get; private set; }
	public void SetTopology ( Topology topology ) {
		if ( topology == Topology )
			return;

		Topology = topology;
		Invalidations |= PipelineInvalidations.Topology;
	}

	public AxisAlignedBox2<uint> Viewport { get; private set; }
	public void SetViewport ( AxisAlignedBox2<uint> viewport ) {
		Viewport = viewport;
		Invalidations |= PipelineInvalidations.Viewport;
	}

	public AxisAlignedBox2<uint> Scissors { get; private set; }
	public void SetScissors ( AxisAlignedBox2<uint> scissors ) {
		Scissors = scissors;
		Invalidations |= PipelineInvalidations.Scissors;
	}

	public BufferTest DepthTest { get; private set; }
	public void SetDepthTest ( BufferTest test ) {
		DepthTest = test;
		Invalidations |= PipelineInvalidations.DepthTest;
	}

	protected BufferInvalidations BufferInvalidations { get; private set; }

	protected IBuffer VertexBuffer { get; private set; } = null!;
	public void BindVertexBuffer ( IBuffer buffer ) {
		VertexBuffer = buffer;
		BufferInvalidations |= BufferInvalidations.Vertex;
	}

	protected IndexBufferType IndexBufferType { get; private set; }
	protected IBuffer IndexBuffer { get; private set; } = null!;
	public void BindIndexBuffer ( IBuffer buffer ) {
		IndexBuffer = buffer;
		IndexBufferType = buffer is IBuffer<uint> ? IndexBufferType.UInt32 : IndexBufferType.UInt16;
		BufferInvalidations |= BufferInvalidations.Index;
	}

	/// <summary>
	/// Updates the pipeline parameters specified by flags in <see cref="Invalidations"/>.
	/// </summary>
	protected abstract void UpdatePieline ( PipelineInvalidations invalidations );

	/// <summary>
	/// Updates the bound buffers.
	/// </summary>
	/// <param name="invalidations">The buffer binds to update. Might be <see cref="BufferInvalidations.None"/>.</param>
	protected abstract void UpdateBuffers ( BufferInvalidations invalidations );

	void ICommandBuffer.DrawIndexed ( uint vertexCount, uint offset ) {
		if ( Invalidations != PipelineInvalidations.None ) {
			UpdatePieline( Invalidations );
			Invalidations = PipelineInvalidations.None;
		}

		const BufferInvalidations binds = BufferInvalidations.Index | BufferInvalidations.Vertex;
		UpdateBuffers( binds & BufferInvalidations );
		BufferInvalidations &= ~binds;

		DrawIndexed( vertexCount, offset );
	}

	/// <inheritdoc cref="ICommandBuffer.DrawIndexed(uint, uint)"/>
	protected abstract void DrawIndexed ( uint vertexCount, uint offset = 0 );
}

[Flags]
public enum PipelineInvalidations : byte {
	None = 0x0,
	Shaders = 0x1,
	Topology = 0x2,
	Viewport = 0x4,
	Scissors = 0x8,
	Framebuffer = 0x10,
	DepthTest = 0x20
}

[Flags]
public enum BufferInvalidations : byte {
	None = 0x0,
	Vertex = 0x1,
	Index  = 0x2
}

public enum IndexBufferType {
	UInt16,
	UInt32
}