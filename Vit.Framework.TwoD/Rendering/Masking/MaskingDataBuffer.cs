using System.Diagnostics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;

namespace Vit.Framework.TwoD.Rendering.Masking;

public class MaskingDataBuffer : IDisposable {
	uint length;
	public MaskingDataBuffer ( uint length ) {
		this.length = length;
	}

	public IBuffer StorageBuffer => stack;
	public uint ByteSize => length * 16;

	IRenderer renderer = null!;
	IHostBuffer stack = null!;
	uint stackPtr;
	public void Initialize ( IRenderer renderer ) {
		this.renderer = renderer;
		stack = renderer.CreateHostBufferRaw<byte>( length * 16, BufferType.ReadonlyStorage, BufferUsage.CpuWrite );
	}

	unsafe void ensureCapacity ( uint size ) {
		if ( stackPtr + size < length )
			return;

		throw new NotImplementedException();
	}

	uint push<T> ( T data ) where T : unmanaged {
		if ( typeof(T) == typeof(MaskingInstruction) ) {
			MaskingInstruction ins = (MaskingInstruction)(object)data;

			if ( ins.Instruction == Instruction.Test ) {
				Debug.Assert( stackPtr % 6 == 0 );
			}
		}

		var size = (SizeOfHelper<T>.Size + 15) / 16; // align to 16B
		ensureCapacity( size );
		stackPtr = (stackPtr + size - 1) / size * size; // align to size as well. this is a temporary fix so indexing is easier.
		stack.UploadRaw( data.ToBytes(), offset: stackPtr * 16 );
		stackPtr += size;
		return stackPtr - size;
	}

	Stack<uint> pushed = new();
	public bool IsEmpty => pushed.Count == 0;
	public uint Peek () => pushed.Peek();

	public uint MaskPointer => IsEmpty ? 0 : Peek();

	/// <summary>
	/// Pushes the masking data and automatically intersects it with the current masking group.
	/// </summary>
	/// <returns>A masking pointer to the resulting mask.</returns>
	public uint Push ( MaskingData data ) {
		push( data );
		uint ptr;
		if ( IsEmpty ) {
			ptr = push( new MaskingInstruction { Instruction = Instruction.Test } );
		}
		else {
			ptr = push( new MaskingInstruction { Instruction = Instruction.Test } );
			ptr = push( new MaskingInstruction { Instruction = Instruction.Intersect, Args = { Item1 = Peek(), Item2 = ptr } } );
		}

		pushed.Push( ptr );
		return ptr;
	}

	/// <summary>
	/// Pops the last added element.
	/// </summary>
	public void Pop () {
		pushed.Pop();
	}

	/// <summary>
	/// Pushes a test.
	/// </summary>
	/// <returns>A masking pointer to the test instruction.</returns>
	public uint PushTest ( MaskingData data ) {
		push( data );
		var ptr = push( new MaskingInstruction { Instruction = Instruction.Test } );
		pushed.Push( ptr );
		return ptr;
	}

	/// <summary>
	/// Pushes an instruction.
	/// </summary>
	/// <returns>A masking pointer to the instruction.</returns>
	public uint PushInstruction ( MaskingInstruction instruction ) {
		Debug.Assert( instruction.Instruction != Instruction.Test );

		var ptr = push( instruction );
		pushed.Push( ptr );
		return ptr;
	}

	public void EndFrame () {
		stackPtr = 0;
		pushed.Clear();
	}

	public void Dispose () {
		stack.Dispose();
	}
}
