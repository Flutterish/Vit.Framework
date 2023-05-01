using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Shaders.Execution;
using Vit.Framework.Graphics.Software.Shaders.Spirv;
using Vit.Framework.Hierarchy;
using Vit.Framework.Interop;
using Version = Vit.Framework.Graphics.Software.Shaders.Spirv.Version;
using Word = System.UInt32;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareShader {
	public readonly SpirvBytecode Spirv;
	public ShaderInfo ShaderInfo => Spirv.Reflections;
	public ShaderPartType Type => Spirv.Type;
	public uint IdMax;

	public List<ShaderSourceLine> SourceLines = new();
	public Dictionary<uint, DataType> DataTypes = new();
	public Dictionary<uint, (uint type, byte[] data)> Constants = new();
	public Dictionary<uint, VariableDeclaration> Variables = new();
	public Dictionary<uint, IntermediateValue> Intermediates = new();

	const Word MagicWord = 0x07230203;
	public SoftwareShader ( SpirvBytecode spirv ) {
		Spirv = spirv;
		var data = spirv.Data;
		var ids = MemoryMarshal.Cast<byte, Word>(data);
		if ( read( ref ids ) != MagicWord )
			throw new Exception( "Invalid Spirv" );

		var version = read( ref ids ).BitCast<Word, Version>();
		// skip generator magic
		ids = ids[1..];
		IdMax = read( ref ids );
		// skip reserved
		ids = ids[1..];

		while ( ids.Length != 0 ) {
			var instruction = read<Instruction>( ref ids );
			var opcode = instruction.OpCode;
			var _data = ids[..( instruction.WordCount - 1 )];
			var words = _data;
			ids = ids[(instruction.WordCount - 1)..];

			if ( opcode == 17 ) {
				// capability
			}
			else if ( opcode == 11 ) {
				// instruction set
			}
			else if ( opcode == 14 ) {
				// model
			}
			else if ( opcode == 15 ) {
				var executionModel = (ExecutionModel)read(ref words);
				var id = read( ref words );
				var name = readString( ref words );
				var interfaces = words.ToArray();
				SourceLines.Add( new() { OpCode = "EntryPoint", Value = $"{name} %{id} | {executionModel} : {string.Join( ", ", interfaces.Select( x => $"%{x}" ) )}" } );
			}
			else if ( opcode == 16 ) {
				var id = read( ref words );
				var executionMode = (ExecutionMode)read( ref words );
				SourceLines.Add( new() { OpCode = "ExecutionMode", Value = $"%{id} | {executionMode} : {string.Join( ", ", words.ToArray() )}" } );
			}
			else if ( opcode is 3 or 4 ) {
				// no semantic meaning
			}
			else if ( opcode == 5 ) {
				var id = read( ref words );
				var name = readString( ref words );
				//SourceLines.Add( new() { OpCode = "SetName", Value = $"%{id} = {name}" } );
			}
			else if ( opcode == 6 ) {
				var id = read( ref words );
				var index = read( ref words );
				var name = readString( ref words );

				//SourceLines.Add( new() { OpCode = "SetName", Value = $"%{id}[{index}] = {name}" } );
			}
			else if ( opcode is 71 or 72 ) {
				// dont care for decorations
			}
			else if ( opcode == 19 ) {
				var id = read( ref words );
				//SourceLines.Add( new() { OpCode = "SetType", Id = id, Value = $"void" } );
				DataTypes.Add( id, new PrimitiveDataType( PrimitiveType.Void ) );
			}
			else if ( opcode == 21 ) {
				var id = read( ref words );
				var width = read( ref words );
				var sign = read( ref words );
				//SourceLines.Add( new() { OpCode = "SetType", Id = id, Value = $"{(sign == 0 ? "u" : "")}int{width}" } );
				DataTypes.Add( id, new PrimitiveDataType( (width, sign) switch {
					(32, not 0 ) => PrimitiveType.Int32,
					(32, 0 ) => PrimitiveType.UInt32,
					_ => throw new Exception( $"Unknown data type: {( sign == 0 ? "u" : "" )}int{width}" )
				} ) );
			}
			else if ( opcode == 22 ) {
				var id = read( ref words );
				var width = read( ref words );
				//SourceLines.Add( new() { OpCode = "SetType", Id = id, Value = $"float{width}" } );
				DataTypes.Add( id, new PrimitiveDataType( width switch {
					32 => PrimitiveType.Float32,
					_ => throw new Exception( $"Unknown data type: float{width}" )
				} ) );
			}
			else if ( opcode == 32 ) {
				var id = read( ref words );
				var storage = (StorageClass)read( ref words );
				var type = read( ref words );
				//SourceLines.Add( new() { OpCode = "SetType", Id = id, Value = $"%{type}* ({storage})" } );
				DataTypes.Add( id, new PointerDataType( type, storage, DataTypes ) );
			}
			else if ( opcode == 23 ) {
				var id = read( ref words );
				var component = read( ref words );
				var count = read( ref words );
				//SourceLines.Add( new() { OpCode = "CreateVector", Id = id, Value = $"%{component}<{count}>" } );
				DataTypes.Add( id, new VectorDataType( component, count, DataTypes ) );
			}
			else if ( opcode == 28 ) {
				var id = read( ref words );
				var component = read( ref words );
				var count = read( ref words );
				//SourceLines.Add( new() { OpCode = "CreateArray", Id = id, Value = $"%{component}[{count}]" } );
				DataTypes.Add( id, new ArrayDataType( component, count, DataTypes ) );
			}
			else if ( opcode == 33 ) {
				var id = read( ref words );
				var returnType = read( ref words );
				var parameters = words.ToArray();
				//SourceLines.Add( new() { OpCode = "SetFunctionType", Id = id, Value = $"%{returnType} ( {string.Join(", ", parameters.Select(x => $"%{x}"))} )" } );
			}
			else if ( opcode == 30 ) {
				var id = read( ref words );
				var members = words.ToArray();
				//SourceLines.Add( new() { OpCode = "DeclareStruct", Id = id, Value = $"{{ {string.Join( ", ", members.Select( x => $"%{x}" ) )} }}" } );
				DataTypes.Add( id, new StructDataType( members, DataTypes ) );
			}
			else if ( opcode == 43 ) {
				var type = read( ref words );
				var id = read( ref words );
				var bytes = MemoryMarshal.Cast<Word, byte>( words );
				//SourceLines.Add( new() { OpCode = "SetConstant", Id = id, Value = $"%{type} 0x{Convert.ToHexString( bytes )}" } );
				Constants.Add( id, (type, bytes.ToArray()) );
			}
			else if ( opcode == 44 ) {
				var type = read( ref words );
				var id = read( ref words );
				var bytes = MemoryMarshal.Cast<Word, byte>( words );
				//SourceLines.Add( new() { OpCode = "SetConstant", Id = id, Value = $"%{type} {string.Join(", ", words.ToArray().Select( x => $"%{x}" ))}" } );
				Constants.Add( id, (type, bytes.ToArray()) );
			}
			else if ( opcode == 59 ) {
				var type = read( ref words );
				var id = read( ref words );
				var storage = (StorageClass)read( ref words );
				uint? initializer = words.Length != 0 ? read( ref words ) : null;
				SourceLines.Add( new() { OpCode = "Variable", Id = id, Value = $"%{type} ({storage})" } );
				Variables.Add( id, new VariableDeclaration( id, type, DataTypes ) );
			}
			else if ( opcode == 54 ) {
				var resultType = read( ref words );
				var id = read( ref words );
				var control = (FunctionControl)read( ref words );
				var funcType = read( ref words );
				SourceLines.Add( new() { OpCode = "Function", Id = id, Value = $"%{funcType} => %{resultType} ({control})" } );
			}
			else if ( opcode == 248 ) {
				var id = read( ref words );
				SourceLines.Add( new() { OpCode = "Label", Id = id } );
			}
			else if ( opcode == 61 ) {
				var type = read( ref words );
				var id = read( ref words );
				var ptr = read( ref words );
				var operands = words.Length != 0 ? (MemoryOperands)read( ref words ) : MemoryOperands.None;
				SourceLines.Add( new() { OpCode = "Load", Id = id, Value = $"%{type} from %{ptr} ({operands})" } );
				Intermediates.Add( id, new IntermediateValue( id, type, DataTypes ) );
			}
			else if ( opcode == 62 ) {
				var ptr = read( ref words );
				var obj = read( ref words );
				var operands = words.Length != 0 ? (MemoryOperands)read( ref words ) : MemoryOperands.None;
				SourceLines.Add( new() { OpCode = "Store", Value = $"%{obj} ({operands}) in %{ptr}" } );
			}
			else if ( opcode == 65 ) {
				var type = read( ref words );
				var id = read( ref words );
				var @base = read( ref words );
				var indices = words.ToArray();
				SourceLines.Add( new() { OpCode = "Index", Id = id, Value = $"%{@base}{string.Join( "", indices.Select( x => $"[%{x}]" ) )} => %{type}" } );
				Intermediates.Add( id, new IntermediateValue( id, type, DataTypes ) );
			}
			else if ( opcode == 80 ) {
				var type = read( ref words );
				var id = read( ref words );
				var elements = words.ToArray();
				SourceLines.Add( new() { OpCode = "Vectorize", Id = id, Value = $"{{{string.Join( ", ", elements.Select( x => $"%{x}" ) )}}} => %{type}" } );
				Intermediates.Add( id, new IntermediateValue( id, type, DataTypes ) );
			}
			else if ( opcode == 81 ) {
				var type = read( ref words );
				var id = read( ref words );
				var composite = read( ref words );
				var indices = words.ToArray();
				SourceLines.Add( new() { OpCode = "Swizzle", Id = id, Value = $"%{composite}[{string.Join( ", ", indices )}] => %{type}" } );
				Intermediates.Add( id, new IntermediateValue( id, type, DataTypes ) );
			}
			else if ( opcode == 253 ) {
				SourceLines.Add( new() { OpCode = "Return" } );
			}
			else if ( opcode == 56 ) {
				SourceLines.Add( new() { OpCode = "FunctionEnd" } );
			}
			else {
				var x = this.ToString();
				Debug.Fail( "oops" );
			}
		}

		var y = this.ToString();
	}

	static string toString ( Span<Word> words ) {
		return Encoding.UTF8.GetString( MemoryMarshal.Cast<Word, byte>( words ) ).TrimEnd( '\0' );
	}

	static string readString ( ref Span<Word> words ) {
		int i = 0;
		while ( words.Length > i && (words[i++] & 0xff) != 0 ) { }
		var str = toString( words[0..i] );
		words = words[i..];
		return str;
	}

	static Word read ( ref Span<Word> words ) {
		var value = words[0];
		words = words[1..];
		return value;
	}

	static T read<T> ( ref Span<Word> words ) where T : unmanaged {
		var value = words[0];
		words = words[1..];
		return value.BitCast<Word, T>();
	}

	static readonly Regex idsRegex = new Regex( @"%\d+", RegexOptions.Compiled );
	public override string ToString () {
		StringBuilder sb = new();
		var width = SourceLines.Count.ToString().Length;
		var idWidth = IdMax.ToString().Length + 6;

		int line = 0;
		foreach ( var i in SourceLines ) {
			var text = i.Value;
			if ( text != null ) {
				foreach ( var id in idsRegex.Matches( i.Value ).Select( x => x.Value ).Distinct() ) {
					var uid = uint.Parse( id[1..] );
					if ( DataTypes.TryGetValue( uid, out var type ) )
						text = text.Replace( id, $"(%{uid}) {type}" );
					else if ( Constants.TryGetValue( uid, out var data ) ) {
						if ( !DataTypes.TryGetValue( data.type, out type ) )
							continue;
						if ( type.Parse( data.data ) is not object obj )
							continue;
						text = text.Replace( id, $"{obj}" );
					}
					else if ( Variables.TryGetValue( uid, out var variable ) ) {
						text = text.Replace( id, $"{variable}" );
					}
					else if ( Intermediates.TryGetValue( uid, out var intermediate ) ) {
						text = text.Replace( id, $"{intermediate}" );
					}
				}
			}
			sb.AppendLine( $"L{(line++).ToString().PadLeft( width, '0' )} {(i.Id != null ? $"%{i.Id} = ".PadLeft(idWidth,' ') : new string(' ', idWidth) )}{i.OpCode}{(string.IsNullOrEmpty(text) ? "" : $" <- {text}" )}" );
		}

		return sb.ToString();
	}
}
