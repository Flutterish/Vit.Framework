﻿using System.Diagnostics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Memory;
using PrimitiveType = Vit.Framework.Graphics.Rendering.Shaders.Reflections.PrimitiveType;

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

	public override void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) {
		((DeviceBuffer<T>)buffer).Upload( data, offset );
	}

	protected override void UploadTextureData<TPixel> ( Texture2D texture, ReadOnlySpan<TPixel> data ) {
		texture.Upload( data );
	}

	protected override void UpdatePieline ( PipelineInvalidations invalidations ) {
		if ( invalidations.HasFlag( PipelineInvalidations.Shaders ) ) {
			GL.UseProgram( ShaderSet.Handle );
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

	int vao;
	DrawElementsType indexType;

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		if ( vao == 0 ) {
			vao = GL.GenVertexArray();
		}

		GL.BindVertexArray( vao );

		foreach ( var (index, set) in ShaderSet.UniformSets ) {
			set.Apply( ShaderSet );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, ((IGlObject)IndexBuffer).Handle );
			indexType = IndexBufferType == IndexBufferType.UInt32 ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
		}

		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			var buffer = (IGlBuffer)VertexBuffer;
			GL.BindBuffer( BufferTarget.ArrayBuffer, buffer.Handle );
			int offset = 0;

			var stride = buffer.Stride;
			foreach ( var i in ShaderSet.Parts.First( x => x.Type == ShaderPartType.Vertex ).ShaderInfo.Input.Resources.OrderBy( x => x.Location ) ) {
				var length = (int)i.Type.FlattendedDimensions;
				var (size, type) = i.Type.PrimitiveType switch {
					PrimitiveType.Float32 => (sizeof( float ), VertexAttribPointerType.Float),
					var x when true => throw new Exception( $"Unknown data type: {x}" )
				};

				GL.VertexAttribPointer( i.Location, length, type, false, stride, offset );
				GL.EnableVertexAttribArray( i.Location );
				offset += length * size;
			}
		}
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Debug.Assert( Topology == Topology.Triangles );
		GL.DrawElements( BeginMode.Triangles, (int)vertexCount, indexType, (int)offset );
	}

	public void Dispose () {
		if ( vao != 0 ) {
			GL.BindVertexArray( 0 );
			GL.DeleteVertexArray( vao );
			vao = 0;
		}
	}
}