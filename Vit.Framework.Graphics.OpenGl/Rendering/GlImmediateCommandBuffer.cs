﻿using System.Diagnostics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Rendering;

public class GlImmediateCommandBuffer : BasicCommandBuffer<GlRenderer, IGlFramebuffer, Texture2D, ShaderProgram>, IImmediateCommandBuffer {
	public GlImmediateCommandBuffer ( GlRenderer renderer ) : base( renderer ) { }

	protected override DisposeAction<ICommandBuffer> RenderTo ( IGlFramebuffer framebuffer, ColorRgba<float> clearColor, float clearDepth, uint clearStencil ) {
		GL.BindFramebuffer( FramebufferTarget.Framebuffer, framebuffer.Handle );

		GL.ClearColor( clearColor.R, clearColor.G, clearColor.B, clearColor.A );
		GL.ClearStencil( (int)clearStencil );
		GL.ClearDepth( clearDepth );
		GL.Clear( ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		} );
	}

	public override void UploadRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint offset = 0 ) {
		((IGlDeviceBuffer)buffer).UploadRaw( data, offset );
	}

	public override void UploadSparseRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint size, uint stride, uint offset = 0 ) {
		((IGlDeviceBuffer)buffer).UploadSparseRaw( data, size, stride, offset );
	}

	protected override void UploadTextureData<TPixel> ( Texture2D texture, ReadOnlySpan<TPixel> data ) {
		texture.Upload( data );
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

	DrawElementsType indexType;

	int[] vertexBuffers = new int[16];
	nint[] bufferOffsets = new nint[16];
	protected override bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset ) {
		var bufferSet = ((IGlBuffer)buffer).Handle.TrySet( ref vertexBuffers[binding] );
		var offsetSet = ((nint)offset).TrySet( ref bufferOffsets[offset] );

		return bufferSet || offsetSet;
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		foreach ( var i in ShaderSet.UniformSets ) {
			i.Apply();
		}

		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			ShaderSet.InputLayout!.BindBuffers( vertexBuffers, bufferOffsets, ShaderSet.InputLayout.BindingPoints );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, ((IGlObject)IndexBuffer).Handle );
			indexType = IndexBufferType == IndexBufferType.UInt32 ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
		}
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Debug.Assert( Topology == Topology.Triangles );
		GL.DrawElements( BeginMode.Triangles, (int)vertexCount, indexType, (int)offset );
	}

	public void Dispose () { }
}