using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Shaders.Spirv;

namespace Vit.Framework.Graphics.Software.Shaders.Execution;

public abstract class DataType {
	public virtual object? Parse ( ReadOnlySpan<byte> bytes ) {
		return null;
	}
}

public class PrimitiveDataType : DataType {
	public PrimitiveType PrimitiveType;

	public override object? Parse ( ReadOnlySpan<byte> bytes ) {
		return PrimitiveType switch {
			PrimitiveType.Float32 => BitConverter.ToSingle( bytes ),
			PrimitiveType.UInt32 => BitConverter.ToUInt32( bytes ),
			PrimitiveType.Int32 => BitConverter.ToInt32( bytes ),
			_ => base.Parse( bytes )
		};
	}

	public PrimitiveDataType ( PrimitiveType primitiveType ) {
		PrimitiveType = primitiveType;
	}

	public override string ToString () {
		return $"{PrimitiveType}";
	}
}

public class PointerDataType : DataType {
	public uint Base;
	public StorageClass StorageClass;
	public Dictionary<uint, DataType> DataTypes;

	public PointerDataType ( uint @base, StorageClass storageClass, Dictionary<uint, DataType> dataTypes ) {
		Base = @base;
		StorageClass = storageClass;
		DataTypes = dataTypes;
	}

	public override string ToString () {
		return $"{( DataTypes.TryGetValue( Base, out var @base ) ? $"{@base}" : $"%{Base}" )}*";
	}
}

public class ArrayDataType : DataType {
	public Dictionary<uint, DataType> DataTypes;
	public uint Base;
	public uint Length;

	public ArrayDataType ( uint @base, uint length, Dictionary<uint, DataType> dataTypes ) {
		DataTypes = dataTypes;
		Base = @base;
		Length = length;
	}

	public override string ToString () {
		return $"{( DataTypes.TryGetValue( Base, out var @base ) ? $"{@base}" : $"%{Base}" )}[{Length}]";
	}
}

public class VectorDataType : DataType {
	public Dictionary<uint, DataType> DataTypes;
	public uint Base;
	public uint Length;

	public VectorDataType ( uint @base, uint length, Dictionary<uint, DataType> dataTypes ) {
		DataTypes = dataTypes;
		Base = @base;
		Length = length;
	}

	public override string ToString () {
		return $"{( DataTypes.TryGetValue( Base, out var @base ) ? $"{@base}" : $"%{Base}" )}<{Length}>";
	}
}

public class StructDataType : DataType {
	public Dictionary<uint, DataType> DataTypes;
	public ImmutableArray<uint> Members;
	public StructDataType ( IEnumerable<uint> members, Dictionary<uint, DataType> dataTypes ) {
		Members = members.ToImmutableArray();
		DataTypes = dataTypes;
	}

	public override string ToString () {
		return $"{{ {string.Join(", ", Members.Select( x => DataTypes.TryGetValue( x, out var type ) ? $"{type}" : $"%{x}" ) )} }}";
	}
}