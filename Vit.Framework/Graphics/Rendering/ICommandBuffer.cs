using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vit.Framework.Exceptions;
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
	DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer );

	/// <summary>
	/// Clears the color of the render target.
	/// </summary>
	void ClearColor<T> ( T color ) where T : IUnlabeledColor<float>; // TODO these may be combined into one operation
	/// <summary>
	/// Clears the depth of the render target.
	/// </summary>
	void ClearDepth ( float depth );
	/// <summary>
	/// Clears the stencil of the render target.
	/// </summary>
	void ClearStencil ( uint stencil );

	/// <summary>
	/// Copies data from one texture to another.
	/// </summary>
	/// <param name="source">The source texture.</param>
	/// <param name="destination">The destination texture.</param>
	/// <param name="sourceRect">The rectangle to copy from source in pixels.</param>
	/// <param name="destinationOffset">The offset into the destination in pixels.</param>
	void CopyTexture ( ITexture2D source, ITexture2D destination, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset );

	/// <summary>
	/// Copies data from one buffer to another.
	/// </summary>
	/// <param name="source">The source buffer.</param>
	/// <param name="destination">The destination buffer.</param>
	/// <param name="length">The amount of data to copy in bytes.</param>
	/// <param name="sourceOffset">Offset into the source buffer in bytes.</param>
	/// <param name="destinationOffset">Offset into the destination buffer in bytes.</param>
	void CopyBufferRaw ( IBuffer source, IBuffer destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 );

	IShaderSet ShaderSet { get; }
	/// <summary>
	/// Sets the shaders for the rendering pipeline. This will also bind or update their uniforms.
	/// </summary>
	void SetShaders ( IShaderSet shaders );

	/// <summary>
	/// Binds or updates the uniform values in the currently bound shader set.
	/// </summary>
	void UpdateUniforms ();

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

	BlendState BlendState { get; }
	/// <summary>
	/// Sets the blending behaviour.
	/// </summary>
	void SetBlending ( BlendState state );

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
	/// <param name="offset">Offset into the buffer in <b>bytes</b>.</param>
	void BindIndexBufferRaw ( IBuffer buffer, IndexBufferType type, uint offset = 0 );

	/// <summary>
	/// Sets the index buffer to use when drawing indexed elements.
	/// </summary>
	/// <param name="offset">Offset into the buffer in <b>elements</b>.</param>
	public void BindIndexBuffer<T> ( IBuffer<T> buffer, uint offset = 0 ) where T : unmanaged {
		if ( typeof( T ) == typeof( ushort ) ) {
			BindIndexBufferRaw( buffer, IndexBufferType.UInt16, offset * sizeof(ushort) );
		}
		else if ( typeof( T ) == typeof( uint ) ) {
			BindIndexBufferRaw( buffer, IndexBufferType.UInt32, offset * sizeof(uint)  );
		}
		else {
			Debug.Assert( false, "Bound an index buffer with an incorrect index type" );
		}
	}

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

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DisposeAction<(ICommandBuffer buffer, BlendState state)> PushBlending ( this ICommandBuffer self, BlendState state ) {
		var previous = self.BlendState;
		self.SetBlending( state );
		return new( (self, previous), static data => {
			data.buffer.SetBlending( data.state );
		} );
	}

	/// <summary>
	/// Copies data from one buffer to another.
	/// </summary>
	/// <param name="source">The source buffer.</param>
	/// <param name="destination">The destination buffer.</param>
	/// <param name="length">The amount of data to copy in elements.</param>
	/// <param name="sourceOffset">Offset into the source buffer in elements.</param>
	/// <param name="destinationOffset">Offset into the destination buffer in elements.</param>
	public static void CopyBuffer<T> ( this ICommandBuffer @this, IBuffer<T> source, IBuffer<T> destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.CopyBufferRaw( source, destination, length * IBuffer<T>.Stride, sourceOffset * IBuffer<T>.Stride, destinationOffset * IBuffer<T>.Stride );
	}
}

/// <summary>
/// Basic implementation of state-tracking for an <see cref="ICommandBuffer"/>.
/// </summary>
public abstract class BasicCommandBuffer<TRenderer, TFramebuffer, TTexture, TShaderSet> : ICommandBuffer
	where TRenderer : class, IRenderer
	where TFramebuffer : class, IFramebuffer
	where TTexture : class, ITexture2D
	where TShaderSet : class, IShaderSet {
	IRenderer ICommandBuffer.Renderer => Renderer;
	protected readonly TRenderer Renderer;
	protected BasicCommandBuffer ( TRenderer renderer ) {
		Renderer = renderer;
	}

	protected TFramebuffer? Framebuffer { get; private set; }
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer ) {
		Framebuffer = (TFramebuffer)framebuffer;
		Invalidations |= PipelineInvalidations.Framebuffer;
		RenderTo( Framebuffer );
		return new DisposeAction<ICommandBuffer>( this, self => {
			var @this = (BasicCommandBuffer<TRenderer, TFramebuffer, TTexture, TShaderSet>)self;
			@this.FinishRendering();
			@this.Framebuffer = null;
		} );
	}
	protected abstract void RenderTo ( TFramebuffer framebuffer );
	protected abstract void FinishRendering ();

	public abstract void ClearColor<T> ( T color ) where T : IUnlabeledColor<float>;
	public abstract void ClearDepth ( float depth );
	public abstract void ClearStencil ( uint stencil );

	public abstract void CopyBufferRaw ( IBuffer source, IBuffer destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 );

	protected abstract void CopyTexture ( TTexture source, TTexture destination, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset );
	public void CopyTexture ( ITexture2D source, ITexture2D destination, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset ) {
		CopyTexture( (TTexture)source, (TTexture)destination, sourceRect, destinationOffset );
	}

	protected PipelineInvalidations Invalidations { get; private set; } = PipelineInvalidations.DepthTest | PipelineInvalidations.StencilTest | PipelineInvalidations.Blending;
	protected void InvalidateAll () {
		uniformsInvalidated = true;
		Invalidations = PipelineInvalidations.All;
		BufferInvalidations = BufferInvalidations.All;
	}

	IShaderSet ICommandBuffer.ShaderSet => ShaderSet;
	protected TShaderSet ShaderSet { get; private set; } = null!;
	public void SetShaders ( IShaderSet shaders ) {
		uniformsInvalidated = true;
		if ( shaders == ShaderSet )
			return;

		ShaderSet = (TShaderSet)shaders;
		Invalidations |= PipelineInvalidations.Shaders;
	}

	bool uniformsInvalidated; // TODO split into "bind" and "update"
	void ICommandBuffer.UpdateUniforms () {
		uniformsInvalidated = true;
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

	public BlendState BlendState { get; private set; }
	public void SetBlending ( BlendState state ) {
		BlendState = state;
		Invalidations |= PipelineInvalidations.Blending;
	}

	protected BufferInvalidations BufferInvalidations { get; private set; }

	public void BindVertexBufferRaw ( IBuffer buffer, uint binding, uint offset ) {
		if ( UpdateVertexBufferMetadata( buffer, binding, offset ) )
			BufferInvalidations |= BufferInvalidations.Vertex;
	}

	protected abstract bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset );

	protected IndexBufferType IndexBufferType { get; private set; }
	protected IBuffer IndexBuffer { get; private set; } = null!;
	protected uint IndexBufferOffset { get; private set; }
	public void BindIndexBufferRaw ( IBuffer buffer, IndexBufferType type, uint offset ) {
		if ( buffer == IndexBuffer && IndexBufferOffset == offset )
			return;

		IndexBuffer = buffer;
		IndexBufferType = type;
		IndexBufferOffset = offset;
		validateIndexType();
		BufferInvalidations |= BufferInvalidations.Index;
	}

	[Conditional("DEBUG")]
	void validateIndexType () {
		if ( IndexBuffer is IBuffer<ushort> && IndexBufferType != IndexBufferType.UInt16 ) {
			throw new InvalidStateException( $"Bound a {IndexBufferType.UInt16} index buffer but decalred it as a {IndexBufferType} buffer" );
		}
		if ( IndexBuffer is IBuffer<uint> && IndexBufferType != IndexBufferType.UInt32 ) {
			throw new InvalidStateException( $"Bound a {IndexBufferType.UInt32} index buffer but decalred it as a {IndexBufferType} buffer" );
		}
	}

	/// <inheritdoc cref="ICommandBuffer.UpdateUniforms"/>
	protected abstract void UpdateUniforms ();

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
		if ( Invalidations != PipelineInvalidations.None ) { // TODO imagine if this was a 10k line long switch stement instead but autogenerated
			UpdatePieline( Invalidations );
			Invalidations = PipelineInvalidations.None;
		}

		if ( uniformsInvalidated ) {
			UpdateUniforms();
			uniformsInvalidated = false;
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
	StencilTest = 0x40,
	Blending = 0x80,
	All = Blending * 2 - 1
}

[Flags]
public enum BufferInvalidations : byte {
	None = 0x0,
	Vertex = 0x1,
	Index  = 0x2,
	All = Index * 2 - 1
}

public enum IndexBufferType {
	UInt16,
	UInt32
}