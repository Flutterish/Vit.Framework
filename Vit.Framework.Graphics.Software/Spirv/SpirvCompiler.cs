using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Instructions;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Types;
using Vit.Framework.Interop;
using Version = Vit.Framework.Graphics.Software.Spirv.Metadata.Version;

namespace Vit.Framework.Graphics.Software.Spirv;

public class SpirvCompiler {
	public MemoryModel? MemoryModel;
	public AddressingModel? AddressingModel;
	public Source? Source;
	public readonly List<string> SourceExtensions = new();

	public readonly Dictionary<uint, string> Names = new();
	public readonly Dictionary<(uint structure, uint member), string> MemberNames = new();
	public readonly Dictionary<uint, Dictionary<DecorationName, Decoration>> Decorations = new();
	public readonly Dictionary<(uint structure, uint member), Dictionary<DecorationName, Decoration>> MemberDecorations = new();

	public readonly List<SpirvInstruction> SpirvInstructions = new();
	public readonly List<Capability> DeclaredCapabilities = new();
	public readonly Dictionary<uint, string> ExtendedInstructionSets = new();
	public readonly Dictionary<ExecutionModel, EntryPoint> EntryPointsByModel = new();
	public readonly Dictionary<uint, EntryPoint> EntryPointsById = new();
	public readonly List<ExecutionMode> ExecutionModes = new();

	public readonly Dictionary<uint, DataType> DataTypes = new();
	public readonly Dictionary<uint, Constant> Constants = new();
	public readonly Dictionary<uint, ConstantComposite> CompositeConstants = new();
	public readonly Dictionary<uint, Variable> Variables = new();
	public readonly Dictionary<uint, Function> Functions = new();

	public readonly Dictionary<uint, IValue> Values = new();
	public readonly Dictionary<uint, IAssignable> Assignables = new();

	public readonly List<Instruction> Instructions = new();

	const uint MagicWord = 0x07230203;
	public SpirvCompiler ( ReadOnlySpan<byte> spirv ) {
		var words = MemoryMarshal.Cast<byte, uint>( spirv );
		var sourceLength = words.Length;
		if ( read( ref words ) != MagicWord )
			throw new Exception( "Invalid Spirv" );

		var version = read<Version>( ref words );
		// skip generator magic
		words = words[1..];
		var IdMax = read( ref words );
		// skip reserved
		words = words[1..];

		while ( words.Length != 0 ) {
			var instruction = read<SpirvInstruction>( ref words );
			var source = new SourceRef() {
				Spirv = instruction.OpCode,
				Line = (uint)Instructions.Count,
				SpirvLine = (uint)(sourceLength - words.Length - 1),
				Compiler = this
			};
			var data = words[..(instruction.WordCount - 1)];
			words = words[(instruction.WordCount - 1)..];

			parseInstruction( source, ref data );
			SpirvInstructions.Add( instruction );

			if ( data.Length != 0 ) {
				Debug.Fail( "Not all operator data consumed" );
			}
		}
	}

	public SoftwareShader Specialise ( ExecutionModel model ) {
		return model switch {
			ExecutionModel.Vertex => new SoftwareVertexShader( this, model ),
			ExecutionModel.Fragment => new SoftwareFragmentShader( this, model ),
			_ => new SoftwareShader( this, model )
		};
	}

	Function? currentFunction;
	static readonly HashSet<OpCode> knownOpCodes = Enum.GetValues<OpCode>().ToHashSet();
	void parseInstruction ( SourceRef source, ref ReadOnlySpan<uint> data ) {
		var code = source.Spirv;

		if ( !knownOpCodes.Contains( code ) ) {
			Debug.Fail( $"Unknown OpCode: {code}" );
		}

		if ( code == OpCode.Capability ) {
			var cap = read<Capability>( ref data );
			DeclaredCapabilities.Add( cap );
		}
		else if ( code == OpCode.ExtInstImport ) {
			ExtendedInstructionSets.Add( read( ref data ), readString( ref data ) );
		}
		else if ( code == OpCode.MemoryModel ) {
			AddressingModel = read<AddressingModel>( ref data );
			MemoryModel = read<MemoryModel>( ref data );
		}
		else if ( code == OpCode.EntryPoint ) {
			var model = read<ExecutionModel>( ref data );
			var entry = new EntryPoint( this ) {
				ExecutionModel = model,
				FunctionId = read( ref data ),
				Name = readString( ref data ),
				InterfaceIds = readArray( ref data )
			};
			EntryPointsByModel.Add( model, entry );
			EntryPointsById.Add( entry.FunctionId, entry );
		}
		else if ( code == OpCode.ExecutionMode ) {
			ExecutionModes.Add( new( this ) { EntryPointId = read( ref data ), Type = read<ExecutionModeType>( ref data ), Data = readArray( ref data ) } );
		}
		else if ( code == OpCode.Source ) {
			Source = new( this ) {
				SourceLanguage = read<SourceLanguage>( ref data ),
				Version = read( ref data ),
				FileNameId = readOptional( ref data ),
				SourceText = readStringOptional( ref data )
			};
		}
		else if ( code == OpCode.SourceExtension ) {
			SourceExtensions.Add( readString( ref data ) );
		}
		else if ( code == OpCode.Name ) {
			Names.Add( read( ref data ), readString( ref data ) );
		}
		else if ( code == OpCode.MemberName ) {
			MemberNames.Add( (read( ref data ), read( ref data )), readString( ref data ) );
		}
		else if ( code is OpCode.Decorate or OpCode.MemberDecorate ) {
			Dictionary<DecorationName, Decoration>? decorations;
			if ( code is OpCode.MemberDecorate ) {
				var key = (read( ref data ), read( ref data ));
				if ( !MemberDecorations.TryGetValue( key, out decorations ) )
					MemberDecorations.Add( key, decorations = new() );
			}
			else {
				var key = read( ref data );
				if ( !Decorations.TryGetValue( key, out decorations ) )
					Decorations.Add( key, decorations = new() );
			}

			var name = read<DecorationName>( ref data );
			decorations.Add( name, new( this ) {
				Name = name,
				Data = readArray( ref data )
			} );
		}
		else if ( code == OpCode.TypeVoid ) {
			var id = read( ref data );
			DataTypes.Add( id, new VoidType( this, id ) );
		}
		else if ( code == OpCode.TypeInt ) {
			var id = read( ref data );
			DataTypes.Add( id, new IntType( this, id ) { Width = read( ref data ), Signed = read( ref data ) != 0 } );
		}
		else if ( code == OpCode.TypeFloat ) {
			var id = read( ref data );
			DataTypes.Add( id, new FloatType( this, id ) { Width = read( ref data ) } );
		}
		else if ( code == OpCode.TypePointer ) {
			var id = read( ref data );
			DataTypes.Add( id, new PointerType( this, id ) { StorageClass = read<StorageClass>( ref data ), TypeId = read( ref data ) } );
		}
		else if ( code == OpCode.TypeVector ) {
			var id = read( ref data );
			DataTypes.Add( id, new VectorType( this, id ) { ComponentTypeId = read( ref data ), Count = read( ref data ) } );
		}
		else if ( code == OpCode.TypeArray ) {
			var id = read( ref data );
			DataTypes.Add( id, new ArrayType( this, id ) { ElementTypeId = read( ref data ), Length = read( ref data ) } );
		}
		else if ( code == OpCode.TypeStruct ) {
			var id = read( ref data );
			DataTypes.Add( id, new StructType( this, id ) { MemberTypeIds = readArray( ref data ) } );
		}
		else if ( code == OpCode.TypeFunction ) {
			var id = read( ref data );
			DataTypes.Add( id, new FunctionType( this, id ) {
				ReturnTypeId = read( ref data ),
				ParameterTypeIds = readArray( ref data )
			} );
		}
		else if ( code == OpCode.Constant ) {
			var type = read( ref data );
			var id = read( ref data );
			var constant = new Constant( this, id ) { DataTypeId = type, Data = readArray( ref data ) };
			Constants.Add( id, constant );

			Values.Add( id, constant );
		}
		else if ( code == OpCode.ConstantComposite ) {
			var type = read( ref data );
			var id = read( ref data );
			var constant = new ConstantComposite( this, id ) { DataTypeId = type, ValueIds = readArray( ref data ) };
			CompositeConstants.Add( id, constant );

			Values.Add( id, constant );
		}
		else if ( code == OpCode.Variable ) {
			var type = read( ref data );
			var id = read( ref data );
			var variable = new Variable( this, id ) { TypeId = type, StorageClass = read<StorageClass>( ref data ), InitializerId = readOptional( ref data ) };
			Variables.Add( id, variable );

			Assignables.Add( id, variable );
			Values.Add( id, variable );
		}
		else if ( code == OpCode.Function ) {
			var type = read( ref data );
			var id = read( ref data );
			Functions.Add( id, currentFunction = new( source, id ) { ReturnTypeId = type, Control = read<FunctionControl>( ref data ), TypeId = read( ref data ) } );
			Instructions.Add( currentFunction );
		}
		else if ( code == OpCode.Label ) {
			var label = new Label( source, read( ref data ) );
			Instructions.Add( label );
			currentFunction!.AddInstruction( label );
		}
		else if ( code == OpCode.Load ) {
			var load = new Load( source ) { ResultTypeId = read( ref data ), ResultId = read( ref data ), PointerId = read( ref data ), MemoryOperands = readOptional<MemoryOperands>( ref data ) };
			Instructions.Add( load );
			currentFunction!.AddInstruction( load );
			ensureResultExists( load.ResultId, load.ResultTypeId );
		}
		else if ( code == OpCode.Store ) {
			var store = new Store( source ) { PointerId = read( ref data ), ObjectId = read( ref data ), MemoryOperands = readOptional<MemoryOperands>( ref data ) };
			Instructions.Add( store );
			currentFunction!.AddInstruction( store );
		}
		else if ( code == OpCode.CompositeExtract ) {
			var extract = new CompositeExtract( source ) { ResultTypeId = read( ref data ), ResultId = read( ref data ), CompositeId = read( ref data ), Indices = readArray( ref data ) };
			Instructions.Add( extract );
			currentFunction!.AddInstruction( extract );
			ensureResultExists( extract.ResultId, extract.ResultTypeId );
		}
		else if ( code == OpCode.CompositeConstruct ) {
			var construct = new CompositeConstruct( source ) { ResultTypeId = read( ref data ), ResultId = read( ref data ), Values = readArray( ref data ) };
			Instructions.Add( construct );
			currentFunction!.AddInstruction( construct );
			ensureResultExists( construct.ResultId, construct.ResultTypeId );
		}
		else if ( code == OpCode.AccessChain ) {
			var chain = new AccessChain( source ) { ResultTypeId = read( ref data ), ResultId = read( ref data ), BaseId = read( ref data ), IndiceIds = readArray( ref data ) };
			Instructions.Add( chain );
			currentFunction!.AddInstruction( chain );
			ensureResultExists( chain.ResultId, chain.ResultTypeId );
		}
		else if ( code == OpCode.Return ) {
			var @return = new Return( source );
			Instructions.Add( @return );
			currentFunction!.AddInstruction( @return );
		}
		else if ( code == OpCode.FunctionEnd ) {
			currentFunction = null;
		}
		else {
			Debug.Fail( "OoopsCode detected" );
		}
	}

	void ensureResultExists ( uint id, uint type ) {
		if ( !Assignables.ContainsKey( id ) ) {
			var intermediate = new Intermediate( this, id ) { TypeId = type };
			Assignables.Add( intermediate.Id, intermediate );
			Values.Add( intermediate.Id, intermediate );
			currentFunction!.Intermediates.Add( intermediate.Id, intermediate );
		}
	}

	static string toString ( ReadOnlySpan<uint> words ) {
		return Encoding.UTF8.GetString( MemoryMarshal.Cast<uint, byte>( words ) ).TrimEnd( '\0' );
	}

	static string readString ( ref ReadOnlySpan<uint> words ) {
		int i = 0;
		while ( words.Length > i && ( words[i++] & 0xff ) != 0 ) { }
		var str = toString( words[0..i] );
		words = words[i..];
		return str;
	}

	static string? readStringOptional ( ref ReadOnlySpan<uint> words ) {
		if ( words.Length == 0 )
			return null;
		return readString( ref words );
	}

	static uint read ( ref ReadOnlySpan<uint> words ) {
		var value = words[0];
		words = words[1..];
		return value;
	}
	static uint? readOptional ( ref ReadOnlySpan<uint> words ) {
		if ( words.Length == 0 )
			return null;
		return read( ref words );
	}

	static T read<T> ( ref ReadOnlySpan<uint> words ) where T : unmanaged {
		var value = words[0];
		words = words[1..];
		return value.BitCast<uint, T>();
	}

	static T? readOptional<T> ( ref ReadOnlySpan<uint> words ) where T : unmanaged {
		if ( words.Length == 0 )
			return null;
		return read<T>( ref words );
	}

	static uint[] readArray ( ref ReadOnlySpan<uint> words ) {
		var arr = words.ToArray();
		words = words = ReadOnlySpan<uint>.Empty;
		return arr;
	}
}
