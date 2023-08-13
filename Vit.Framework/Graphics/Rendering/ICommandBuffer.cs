﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
	IRenderer Renderer { get; }

	/// <summary>
	/// Starts rendering to a frame buffer.
	/// </summary>
	/// <returns>An action which will finish rendering to the framebuffer.</returns>
	DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null );

	/// <inheritdoc cref="IHostBuffer.UploadRaw(ReadOnlySpan{byte}, uint)"/>
	/// <param name="buffer">The device buffer to upload to.</param>
	void UploadRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint offset = 0 );

	/// <inheritdoc cref="IHostBuffer.UploadSparseRaw(ReadOnlySpan{byte}, uint, uint, uint)"/>
	/// <param name="buffer">The device buffer to upload to.</param>
	void UploadSparseRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint size, uint stride, uint offset = 0 );

	/// <inheritdoc cref="IHostBuffer{T}.Upload(ReadOnlySpan{T}, uint)"/>
	/// <param name="buffer">The device buffer to upload to.</param>
	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		UploadRaw( buffer, MemoryMarshal.AsBytes( data ), offset * IBuffer<T>.Stride );
	}

	/// <inheritdoc cref="IHostBuffer{T}.UploadUniform(ReadOnlySpan{T}, uint)"/>
	/// <param name="buffer">The device buffer to upload to.</param>
	public void UploadUniform<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		UploadAligned( buffer, data, 256, offset );
	}

	/// <inheritdoc cref="IHostBuffer{T}.UploadAligned(ReadOnlySpan{T}, uint, uint)"/>
	/// <param name="buffer">The device buffer to upload to.</param>
	public void UploadAligned<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint alignment, uint offset = 0 ) where T : unmanaged {
		var stride = (IBuffer<T>.Stride + alignment - 1) / alignment * alignment;
		UploadSparseRaw( buffer, MemoryMarshal.AsBytes( data ), IBuffer<T>.Stride, stride, offset * stride );
	}

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
	DepthState DepthState { get; }
	/// <summary>
	/// Sets the depth testing behaviour.
	/// </summary>
	void SetDepthTest ( BufferTest test, DepthState state );

	BufferTest StencilTest { get; }
	StencilState StencilState { get; }
	/// <summary>
	/// Sets the stencil testing behaviour.
	/// </summary>
	void SetStencilTest ( BufferTest test, StencilState state );

	/// <summary>
	/// Binds a vertex buffer to the pipeline.
	/// </summary>
	/// <param name="offset">Offset into the buffer in <b>bytes</b>.</param>
	void BindVertexBufferRaw ( IBuffer buffer, uint binding = 0, uint offset = 0 );

	/// <summary>
	/// Binds a vertex buffer to the pipeline.
	/// </summary>
	/// <param name="offset">Offset into the buffer in <b>elements</b>.</param>
	public void BindVertexBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		BindVertexBufferRaw( buffer, binding, offset * IBuffer<T>.Stride );
	}

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
	public static DisposeAction<(ICommandBuffer buffer, (BufferTest test, DepthState state) depth)> PushDepthTest ( this ICommandBuffer self, BufferTest depthTest, DepthState state ) {
		var previous = (self.DepthTest, self.DepthState);
		self.SetDepthTest( depthTest, state );
		return new( (self, previous), static data => {
			data.buffer.SetDepthTest( data.depth.test, data.depth.state );
		} );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, (BufferTest test, StencilState state) stencil)> PushStencilTest ( this ICommandBuffer self, BufferTest stencilTest, StencilState state ) {
		var previous = (self.StencilTest, self.StencilState);
		self.SetStencilTest( stencilTest, state );
		return new( (self, previous), static data => {
			data.buffer.SetStencilTest( data.stencil.test, data.stencil.state );
		} );
	}
}

/// <summary>
/// Basic implementation of state-tracking for an <see cref="ICommandBuffer"/>.
/// </summary>
public abstract class BasicCommandBuffer<TRenderer, TFramebuffer, TTexture, TShaderSet> : ICommandBuffer
	where TRenderer : class, IRenderer
	where TFramebuffer : class, IFramebuffer
	where TTexture : class, ITexture
	where TShaderSet : class, IShaderSet 
{
	IRenderer ICommandBuffer.Renderer => Renderer;
	protected readonly TRenderer Renderer;
	protected BasicCommandBuffer ( TRenderer renderer ) {
		Renderer = renderer;
	}

	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		Invalidations |= PipelineInvalidations.Framebuffer;
		return RenderTo( (TFramebuffer)framebuffer, clearColor ?? ColorRgba.Black, clearDepth ?? 0, clearStencil ?? 0 );
	}
	protected abstract DisposeAction<ICommandBuffer> RenderTo ( TFramebuffer framebuffer, ColorRgba<float> clearColor, float clearDepth, uint clearStencil );

	public abstract void UploadRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint offset = 0 );
	public abstract void UploadSparseRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint size, uint stride, uint offset = 0 );

	public void UploadTextureData<TPixel> ( ITexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		UploadTextureData( (TTexture)texture, data );
	}
	protected abstract void UploadTextureData<TPixel> ( TTexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged;

	protected PipelineInvalidations Invalidations { get; private set; } = PipelineInvalidations.DepthTest | PipelineInvalidations.StencilTest;

	IShaderSet ICommandBuffer.ShaderSet => ShaderSet;
	protected TShaderSet ShaderSet { get; private set; } = null!;
	public void SetShaders ( IShaderSet shaders ) { // TODO we need to detect uniform changes so we don't waste time binding them again
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
	public DepthState DepthState { get; private set; }
	public void SetDepthTest ( BufferTest test, DepthState state ) {
		DepthTest = test;
		DepthState = state;
		Invalidations |= PipelineInvalidations.DepthTest;
	}

	public BufferTest StencilTest { get; private set; }
	public StencilState StencilState { get; private set; }
	public void SetStencilTest ( BufferTest test, StencilState state ) {
		StencilTest = test;
		StencilState = state;
		Invalidations |= PipelineInvalidations.StencilTest;
	}

	protected BufferInvalidations BufferInvalidations { get; private set; }

	public void BindVertexBufferRaw ( IBuffer buffer, uint binding, uint offset ) {
		if ( UpdateVertexBufferMetadata( buffer, binding, offset ) )
			BufferInvalidations |= BufferInvalidations.Vertex;
	}

	protected abstract bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset );

	protected IndexBufferType IndexBufferType { get; private set; }
	protected IBuffer IndexBuffer { get; private set; } = null!;
	public void BindIndexBuffer ( IBuffer buffer ) {
		if ( buffer == IndexBuffer )
			return;

		IndexBuffer = buffer;
		IndexBufferType = buffer is IBuffer<uint> ? IndexBufferType.UInt32 : IndexBufferType.UInt16; // TODO bad!
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
		validateAlignment();
		UpdateBuffers( binds & BufferInvalidations );
		BufferInvalidations &= ~binds;

		DrawIndexed( vertexCount, offset );
	}

	[Conditional("DEBUG")]
	void validateAlignment () {
		// TODO check attribute linking
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
	DepthTest = 0x20,
	StencilTest = 0x40
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