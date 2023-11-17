using System.Diagnostics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.OpenGl.Rendering;

public class GlImmediateCommandBuffer : BasicCommandBuffer<GlRenderer, IGlFramebuffer, IGlTexture2D, ShaderProgram>, IImmediateCommandBuffer {
	public GlImmediateCommandBuffer ( GlRenderer renderer ) : base( renderer ) { }

	protected override void RenderTo ( IGlFramebuffer framebuffer ) {
		GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, framebuffer.Handle );
	}

	protected override void FinishRendering () {
		GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0 );
	}

	public override void ClearColor<T> ( T color ) {
		var span = color.AsSpan();
		GL.ClearColor(
			span.Length >= 1 ? span[0] : 0,
			span.Length >= 2 ? span[1] : 0,
			span.Length >= 3 ? span[2] : 0,
			span.Length >= 4 ? span[3] : 1
		);
		GL.Clear( ClearBufferMask.ColorBufferBit );
	}

	public override void ClearDepth ( float depth ) {
		GL.ClearDepth( depth );
		GL.Clear( ClearBufferMask.DepthBufferBit );
	}

	public override void ClearStencil ( uint stencil ) {
		GL.ClearStencil( (int)stencil );
		GL.Clear( ClearBufferMask.StencilBufferBit );
	}

	protected override void CopyTexture ( IGlTexture2D source, IGlTexture2D destination, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset ) {
		Debug.Assert( source.Format == Graphics.Rendering.Textures.PixelFormat.Rgba8 );
		Debug.Assert( destination.Format == Graphics.Rendering.Textures.PixelFormat.Rgba8 );

		switch ( (source.Type, destination.Type) ) {
			case (GlTextureType.PixelBuffer, GlTextureType.Storage):
				var src = (PixelBuffer)source;
				var dst = (Texture2DStorage)destination;

				GL.BindBuffer( BufferTarget.PixelUnpackBuffer, src.Handle );
				GL.PixelStore( PixelStoreParameter.UnpackRowLength, (int)src.Size.Width );
				nint offset = (nint)(sizeof( byte ) * 4 * ( sourceRect.MinX + sourceRect.MinY * source.Size.Width ));
				GL.TextureSubImage2D( dst.Handle, 0, (int)destinationOffset.X, (int)destinationOffset.Y, (int)sourceRect.Width, (int)sourceRect.Height, PixelFormat.Rgba, PixelType.UnsignedByte, offset );
				GL.BindBuffer( BufferTarget.PixelUnpackBuffer, 0 );
				break;

			default:
				throw new NotImplementedException();
		}
	}

	public override void CopyBufferRaw ( IBuffer source, IBuffer destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		var src = (IGlBuffer)source;
		var dst = (IGlBuffer)destination;

		GL.CopyNamedBufferSubData( src.Handle, dst.Handle, (nint)sourceOffset, (nint)destinationOffset, (int)length );
	}

	protected override void UpdatePieline ( PipelineInvalidations invalidations ) {
		if ( invalidations.HasFlag( PipelineInvalidations.Shaders ) ) {
			GL.UseProgram( ShaderSet.Handle );
			ShaderSet.InputLayout!.BindVAO();
		}

		if ( invalidations.HasFlag( PipelineInvalidations.Viewport ) )
			GL.Viewport( (int)Viewport.MinX, (int)Viewport.MinY, (int)Viewport.Width, (int)Viewport.Height );

		if ( invalidations.HasFlag( PipelineInvalidations.Scissors ) )
			GL.Scissor( (int)Scissors.MinX, (int)Scissors.MinY, (int)Scissors.Width, (int)Scissors.Height );

		if ( invalidations.HasFlag( PipelineInvalidations.DepthTest ) ) {
			if ( !DepthTest.IsEnabled ) {
				GL.Disable( EnableCap.DepthTest );
				goto stencil;
			}

			GL.Enable( EnableCap.DepthTest );
			GL.DepthMask( DepthState.WriteOnPass );
			GL.DepthFunc( DepthTest.CompareOperation switch {
				CompareOperation.LessThan => DepthFunction.Less,
				CompareOperation.GreaterThan => DepthFunction.Greater,
				CompareOperation.Equal => DepthFunction.Equal,
				CompareOperation.NotEqual => DepthFunction.Notequal,
				CompareOperation.LessThanOrEqual => DepthFunction.Lequal,
				CompareOperation.GreaterThanOrEqual => DepthFunction.Gequal,
				CompareOperation.Always => DepthFunction.Always,
				CompareOperation.Never or _ => DepthFunction.Never
			} );
		}
		
		stencil:
		if ( invalidations.HasFlag( PipelineInvalidations.StencilTest ) ) {
			if ( !StencilTest.IsEnabled ) {
				GL.Disable( EnableCap.StencilTest );
				return;
			}

			GL.Enable( EnableCap.StencilTest );
			GL.StencilMask( StencilState.WriteMask );
			GL.StencilFunc( StencilTest.CompareOperation switch {
				CompareOperation.LessThan => StencilFunction.Less,
				CompareOperation.GreaterThan => StencilFunction.Greater,
				CompareOperation.Equal => StencilFunction.Equal,
				CompareOperation.NotEqual => StencilFunction.Notequal,
				CompareOperation.LessThanOrEqual => StencilFunction.Lequal,
				CompareOperation.GreaterThanOrEqual => StencilFunction.Gequal,
				CompareOperation.Always => StencilFunction.Always,
				CompareOperation.Never or _ => StencilFunction.Never
			}, (int)StencilState.ReferenceValue, StencilState.CompareMask );
			static StencilOp stencilOp ( StencilOperation operation ) {
				return operation switch {
					StencilOperation.SetTo0 => StencilOp.Zero,
					StencilOperation.ReplaceWithReference => StencilOp.Replace,
					StencilOperation.Invert => StencilOp.Invert,
					StencilOperation.Increment => StencilOp.Incr,
					StencilOperation.Decrement => StencilOp.Decr,
					StencilOperation.IncrementWithWrap => StencilOp.IncrWrap,
					StencilOperation.DecrementWithWrap => StencilOp.DecrWrap,
					StencilOperation.Keep or _ => StencilOp.Keep
				};
			}
			GL.StencilOp( stencilOp( StencilState.StencilFailOperation ), stencilOp( StencilState.DepthFailOperation ), stencilOp( StencilState.PassOperation ) );
		}
	}

	protected override void UpdateUniforms () {
		foreach ( var i in ShaderSet.UniformSets ) {
			i.Apply();
		}
	}

	DrawElementsType indexType;

	int[] vertexBuffers = new int[16];
	nint[] bufferOffsets = new nint[16];
	protected override bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset ) {
		var bufferSet = ((IGlBuffer)buffer).Handle.TrySet( ref vertexBuffers[binding] );
		var offsetSet = ((nint)offset).TrySet( ref bufferOffsets[offset] );

		return bufferSet || offsetSet;
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			ShaderSet.InputLayout!.BindBuffers( vertexBuffers, bufferOffsets, ShaderSet.InputLayout.BindingPoints );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, ((IGlObject)IndexBuffer).Handle );
			indexType = IndexBufferType == IndexBufferType.UInt32 ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
		}
	}

	[ThreadStatic]
	static GlImmediateCommandBuffer? activeState;
	void ensureActiveState () {
		Debug.Assert( activeState == this || activeState == null );
		activeState = this;
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		ensureActiveState();

		Debug.Assert( Topology == Topology.Triangles );
		GL.DrawElements( BeginMode.Triangles, (int)vertexCount, indexType, (int)offset );
	}

	public void Dispose () { }
}