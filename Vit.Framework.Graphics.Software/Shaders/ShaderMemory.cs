﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Software.Shaders;

[DebuggerDisplay( "{ToString(),nq}" )]
public struct VariableInfo {
	public IRuntimeType Type;
	public int Address;

	public override string ToString () {
		return $"{Type} at 0x{Address:X}";
	}
}

[DebuggerDisplay( "{ToString(),nq}" )]
public struct MemoryDebugInfo {
	public VariableInfo Variable;
	public string Name;

	public override string ToString () {
		return $"{Name} : {Variable.Type} at 0x{Variable.Address:X}";
	}
}

public class MemoryDebugFrame {
	public MemoryDebugFrame? ParentFrame;
	public Dictionary<int, MemoryDebugInfo> Info = new();
	public int StackPointerOffset;

	public void Add ( MemoryDebugInfo info ) {
		info.Variable.Address -= StackPointerOffset;
		Info.Add( info.Variable.Address, info );
	}
}

public ref struct ShaderMemory { // TODO consider baked debug information instead
	public Span<byte> Memory;
	public int StackPointer;

#if SHADER_DEBUG
	public Dictionary<int, MemoryDebugInfo>? DebugInfo;
	public void AddDebug ( MemoryDebugInfo info ) {
		DebugInfo ??= new();
		DebugInfo[info.Variable.Address] = info;
	}
#endif

	public Span<byte> GetMemory ( int offset, int length )
		=> Memory.Slice( offset, length );

	public unsafe T* GetPointer<T> ( int address ) where T : unmanaged {
		return (T*)( Memory.Data() + address );
	}

	public unsafe void* GetPointer ( int address ) {
		return Memory.Data() + address;
	}

	public T Read<T> ( int address ) where T : unmanaged {
		return MemoryMarshal.Read<T>( Memory[address..] );
	}

	public void Write<T> ( int address, T value ) where T : unmanaged {
		MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref value, 1 ) ).CopyTo( Memory[address..] );
	}

	public void Copy ( int from, VariableInfo to ) {
		GetMemory( from, to.Type.Size ).CopyTo( Memory[to.Address..] );
	}

	public void Copy ( VariableInfo from, int to ) {
		GetMemory( from.Address, from.Type.Size ).CopyTo( Memory[to..] );
	}

	public VariableInfo StackAlloc ( IRuntimeType type ) {
		var ptr = StackPointer;
		StackPointer += type.Size;
		return new() { Address = ptr, Type = type };
	}

#if SHADER_DEBUG
	public override string ToString () {
		var memoryLength = Memory.Length.ToString( "X" ).Length;

		var sb = new StringBuilder();
		sb.AppendLine( $"Shader Memory [{Memory.Length}B] ({StackPointer}B used, {Memory.Length - StackPointer}B free)" );
		if ( DebugInfo is null )
			return sb.ToString();

		var stackPtr = StackPointer;
		foreach ( var (address, i) in DebugInfo.Where( x => x.Key < stackPtr ).OrderBy( x => x.Key ) ) {
			string value;
			if ( i.Variable.Type is RuntimePointerType ) {
				value = "0x" + Read<int>( i.Variable.Address ).ToString( "X" ).PadLeft( memoryLength, '0' );
			}
			else {
				var variable = i.Variable.Type.CreateVariable();
				variable.Parse( GetMemory( i.Variable.Address, i.Variable.Type.Size ) );
				value = variable.ToString()!;
			}
			
			sb.AppendLine( $"0x{i.Variable.Address.ToString("X").PadLeft(memoryLength, '0')}\t{i.Name} : {i.Variable.Type} = {value}" );
		}
		return sb.ToString();
	}

	[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
	private List<string> DebugView {
		get {
			List<string> view = new();
			if ( DebugInfo == null )
				return view;

			var memoryLength = Memory.Length.ToString( "X" ).Length;
			var stackPtr = StackPointer;
			foreach ( var (address, i) in DebugInfo.Where( x => x.Key < stackPtr ).OrderBy( x => x.Key ) ) {
				string value;
				if ( i.Variable.Type is RuntimePointerType ) {
					value = "0x" + Read<int>( i.Variable.Address ).ToString( "X" ).PadLeft( memoryLength, '0' );
				}
				else {
					var variable = i.Variable.Type.CreateVariable();
					variable.Parse( GetMemory( i.Variable.Address, i.Variable.Type.Size ) );
					value = variable.ToString()!;
				}

				view.Add( $"0x{i.Variable.Address.ToString( "X" ).PadLeft( memoryLength, '0' )} {i.Name} : {i.Variable.Type} = {value}" );
			}
			return view;
		}
	}
#endif
}