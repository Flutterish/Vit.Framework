using System.Diagnostics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.OpenGl.Rendering;

public class GlImmediateCommandBuffer : BasicCommandBuffer<GlRenderer, IGlFramebuffer, IGlTexture2D, ShaderProgram>, IImmediateCommandBuffer {
	[ThreadStatic]
	static GlImmediateCommandBuffer? activeState;

	public GlImmediateCommandBuffer ( GlRenderer renderer ) : base( renderer ) {
		activeState?.InvalidateAll();
		activeState = this;

		GL.Enable( EnableCap.ScissorTest );
	}

	[ThreadStatic]
	static int currentFrameBuffer;
	int previousFrameBuffer;
	protected override void RenderTo ( IGlFramebuffer framebuffer ) {
		previousFrameBuffer = currentFrameBuffer;
		GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, currentFrameBuffer = framebuffer.Handle );
	}

	protected override void FinishRendering () {
		GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, currentFrameBuffer = previousFrameBuffer );
	}

	public override void ClearColor<T> ( T color ) {
		GL.Disable( EnableCap.ScissorTest );
		var span = color.AsSpan();
		GL.ClearColor(
			span.Length >= 1 ? span[0] : 0,
			span.Length >= 2 ? span[1] : 0,
			span.Length >= 3 ? span[2] : 0,
			span.Length >= 4 ? span[3] : 1
		);
		GL.Clear( ClearBufferMask.ColorBufferBit );
		GL.Enable( EnableCap.ScissorTest );
	}

	public override void ClearDepth ( float depth ) {
		GL.Disable( EnableCap.ScissorTest );
		GL.ClearDepth( depth );
		GL.Clear( ClearBufferMask.DepthBufferBit );
		GL.Enable( EnableCap.ScissorTest );
	}

	public override void ClearStencil ( uint stencil ) {
		GL.Disable( EnableCap.ScissorTest );
		GL.ClearStencil( (int)stencil );
		GL.Clear( ClearBufferMask.StencilBufferBit );
		GL.Enable( EnableCap.ScissorTest );
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

		if ( invalidations.HasFlag( PipelineInvalidations.Viewport ) ) {
			GL.Viewport( (int)Viewport.MinX, (int)Viewport.MinY, (int)Viewport.Width, (int)Viewport.Height );
		}

		if ( invalidations.HasFlag( PipelineInvalidations.Scissors ) ) {
			GL.Scissor( (int)Scissors.MinX, (int)Scissors.MinY, (int)Scissors.Width, (int)Scissors.Height );
		}

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
				goto blending;
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

		blending:
		if ( invalidations.HasFlag( PipelineInvalidations.Blending ) ) {
			if ( !BlendState.IsEnabled ) {
				GL.Disable( EnableCap.Blend );
				return;
			}

			static BlendEquationMode equationMode ( BlendFunction function ) {
				return function switch {
					BlendFunction.Add => BlendEquationMode.FuncAdd,
					BlendFunction.Max => BlendEquationMode.Max,
					BlendFunction.Min => BlendEquationMode.Min,
					BlendFunction.FragmentMinusDestination => BlendEquationMode.FuncSubtract,
					BlendFunction.DestinationMinusFragment or _ => BlendEquationMode.FuncReverseSubtract
				};
			}

			static BlendingFactorSrc factorSrc ( BlendFactor factor ) {
				return factor switch {
					BlendFactor.Zero => BlendingFactorSrc.Zero,
					BlendFactor.One => BlendingFactorSrc.One,
					BlendFactor.Fragment => BlendingFactorSrc.SrcColor,
					BlendFactor.FragmentInverse => BlendingFactorSrc.OneMinusSrcColor,
					BlendFactor.Destination => BlendingFactorSrc.DstColor,
					BlendFactor.DestinationInverse => BlendingFactorSrc.OneMinusDstColor,
					BlendFactor.FragmentAlpha => BlendingFactorSrc.SrcAlpha,
					BlendFactor.FragmentAlphaInverse => BlendingFactorSrc.OneMinusSrcAlpha,
					BlendFactor.DestinationAlpha => BlendingFactorSrc.DstAlpha,
					BlendFactor.DestinationAlphaInverse => BlendingFactorSrc.OneMinusDstAlpha,
					BlendFactor.Constant => BlendingFactorSrc.ConstantColor,
					BlendFactor.ConstantInverse => BlendingFactorSrc.OneMinusConstantColor,
					BlendFactor.ConstantAlpha => BlendingFactorSrc.ConstantAlpha,
					BlendFactor.ConstantAlphaInverse => BlendingFactorSrc.OneMinusConstantAlpha,
					BlendFactor.AlphaSaturate => BlendingFactorSrc.SrcAlphaSaturate,
					BlendFactor.SecondFragment => BlendingFactorSrc.Src1Color,
					BlendFactor.SecondFragmentAlpha => BlendingFactorSrc.Src1Alpha,
					BlendFactor.SecondFragmentInverse => BlendingFactorSrc.OneMinusSrc1Color,
					BlendFactor.SecondFragmentAlphaInverse or _ => BlendingFactorSrc.OneMinusSrc1Alpha
				};
			}

			static BlendingFactorDest factorDst ( BlendFactor factor ) {
				return factor switch {
					BlendFactor.Zero => BlendingFactorDest.Zero,
					BlendFactor.One => BlendingFactorDest.One,
					BlendFactor.Fragment => BlendingFactorDest.SrcColor,
					BlendFactor.FragmentInverse => BlendingFactorDest.OneMinusSrcColor,
					BlendFactor.Destination => BlendingFactorDest.DstColor,
					BlendFactor.DestinationInverse => BlendingFactorDest.OneMinusDstColor,
					BlendFactor.FragmentAlpha => BlendingFactorDest.SrcAlpha,
					BlendFactor.FragmentAlphaInverse => BlendingFactorDest.OneMinusSrcAlpha,
					BlendFactor.DestinationAlpha => BlendingFactorDest.DstAlpha,
					BlendFactor.DestinationAlphaInverse => BlendingFactorDest.OneMinusDstAlpha,
					BlendFactor.Constant => BlendingFactorDest.ConstantColor,
					BlendFactor.ConstantInverse => BlendingFactorDest.OneMinusConstantColor,
					BlendFactor.ConstantAlpha => BlendingFactorDest.ConstantAlpha,
					BlendFactor.ConstantAlphaInverse => BlendingFactorDest.OneMinusConstantAlpha,
					BlendFactor.AlphaSaturate => BlendingFactorDest.SrcAlphaSaturate,
					BlendFactor.SecondFragment => BlendingFactorDest.Src1Color,
					BlendFactor.SecondFragmentAlpha => BlendingFactorDest.Src1Alpha,
					BlendFactor.SecondFragmentInverse => BlendingFactorDest.OneMinusSrc1Color,
					BlendFactor.SecondFragmentAlphaInverse or _ => BlendingFactorDest.OneMinusSrc1Alpha
				};
			}

			GL.Enable( EnableCap.Blend );
			GL.BlendEquationSeparate( equationMode( BlendState.ColorFunction ), equationMode( BlendState.AlphaFunction ) );
			GL.BlendFuncSeparate( factorSrc( BlendState.FragmentColorFactor ), factorDst( BlendState.DestinationColorFactor ), factorSrc( BlendState.FragmentAlphaFactor ), factorDst( BlendState.DestinationAlphaFactor ) );
			GL.BlendColor( BlendState.Constant.X, BlendState.Constant.Y, BlendState.Constant.Z, BlendState.Constant.W );
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
		var offsetSet = ((nint)offset).TrySet( ref bufferOffsets[binding] );

		return bufferSet || offsetSet;
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			ShaderSet.InputLayout!.BindBuffers( vertexBuffers, bufferOffsets );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, ((IGlObject)IndexBuffer).Handle );
			indexType = IndexBufferType == IndexBufferType.UInt32 ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
		}
	}

	static uint indexSize ( uint count, DrawElementsType type ) {
		return count * type switch {
			DrawElementsType.UnsignedByte => 1u,
			DrawElementsType.UnsignedShort => 2u,
			DrawElementsType.UnsignedInt or _ => 4u
		};
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Debug.Assert( Topology == Topology.Triangles );
		GL.DrawElements( BeginMode.Triangles, (int)vertexCount, indexType, (int)(IndexBufferOffset + indexSize(offset, indexType)) );
	}

	protected override void DrawInstancesIndexed ( uint vertexCount, uint instanceCount, uint offset, uint instanceOffset ) {
		Debug.Assert( Topology == Topology.Triangles );
		GL.DrawElementsInstancedBaseInstance( PrimitiveType.Triangles, (int)vertexCount, indexType, (int)(IndexBufferOffset + indexSize( offset, indexType )), (int)instanceCount, instanceOffset );
	}

	public void Dispose () {

	}
}