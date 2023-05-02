using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface IVariable {
	IRuntimeType Type { get; }
	object? Value { get; set; }

	void Parse ( ReadOnlySpan<byte> data );

	void Interpolate ( float a, float b, float c, IVariable A, IVariable B, IVariable C );
}

public interface IVariable<T> : IVariable where T : unmanaged {
	IRuntimeType IVariable.Type => Type;
	new IRuntimeType<T> Type { get; }
	object? IVariable.Value {
		get => Value;
		set => Value = (T)value!;
	}

	new T Value { get; set; }

	void IVariable.Parse ( ReadOnlySpan<byte> data ) => Value = MemoryMarshal.Read<T>( data );

	void IVariable.Interpolate ( float a, float b, float c, IVariable A, IVariable B, IVariable C )
		=> Interpolate( a, b, c, (IVariable<T>)A, (IVariable<T>)B, (IVariable<T>)C );
	void Interpolate ( float a, float b, float c, IVariable<T> A, IVariable<T> B, IVariable<T> C );
}

public class NumericVariable<T> : IVariable<T> where T : unmanaged, INumber<T> {
	public IRuntimeType<T> Type { get; }
	public NumericVariable ( IRuntimeType<T> type ) {
		Type = type;
	}

	public T Value { get; set; }

	public override string ToString () {
		return $"{Value}";
	}

	public void Interpolate ( float a, float b, float c, IVariable<T> A, IVariable<T> B, IVariable<T> C ) {
		Value = T.CreateChecked(float.CreateTruncating( A.Value ) * a + float.CreateTruncating( B.Value ) * b + float.CreateTruncating( C.Value ) * c);
	}
}

public interface ICompositeVariable : IVariable {
	public IVariable this[uint index] { get; }
}

public class StructVariable : ICompositeVariable {
	public RuntimeStructType Type;
	public StructVariable ( RuntimeStructType type ) {
		Type = type;
		Members = type.Members.Select( x => x.CreateVariable() ).ToArray();
	}

	public IVariable[] Members;

	IRuntimeType IVariable.Type => Type;
	public object? Value {
		get => Members;
		set {
			throw new NotImplementedException();
		}
	}

	public void Parse ( ReadOnlySpan<byte> data ) {
		throw new NotImplementedException();
	}


	public override string ToString () {
		return $"{{{string.Join(", ", Members.AsEnumerable())}}}";
	}

	public IVariable this[uint index] => Members[index];

	public void Interpolate ( float a, float b, float c, IVariable A, IVariable B, IVariable C ) {
		throw new NotImplementedException();
	}
}

public abstract class VectorVariable<T, TVector> : IVariable<TVector>, ICompositeVariable where T : unmanaged, INumber<T> where TVector : unmanaged {
	public IRuntimeType<TVector> Type { get; }
	public IRuntimeType<T> ComponentType { get; }
	public VectorVariable ( IRuntimeType<TVector> type, IRuntimeType<T> componentType, int length ) {
		Type = type;
		ComponentType = componentType;
		Components = Enumerable.Range(0, length).Select( x => componentType.CreateVariable() ).ToArray();
	}

	public readonly IVariable<T>[] Components;

	public void Parse ( ReadOnlySpan<byte> data ) {
		var size = ComponentType.Size;
		foreach ( var i in Components ) {
			i.Parse( data[..size] );
			data = data[size..];
		}
	}

	public IVariable this[uint index] => Components[index];
	public abstract TVector Value { get; set; }

	public override string ToString () {
		return $"<{string.Join(", ", Components.AsEnumerable())}>";
	}

	public void Interpolate ( float a, float b, float c, IVariable<TVector> A, IVariable<TVector> B, IVariable<TVector> C ) {
		for ( int i = 0; i < Components.Length; i++ ) {
			Components[i].Interpolate( a, b, c, ((VectorVariable<T, TVector>)A).Components[i], ((VectorVariable<T, TVector>)B).Components[i], ((VectorVariable<T, TVector>)C).Components[i] );
		}
	}
}

public class Vector2Variable<T> : VectorVariable<T, Vector2<T>> where T : unmanaged, INumber<T> {
	public Vector2Variable ( IRuntimeType<Vector2<T>> type, IRuntimeType<T> componentType ) : base( type, componentType, 2 ) { }

	public override Vector2<T> Value {
		get => new( Components[0].Value, Components[1].Value );
		set {
			Components[0].Value = value.X;
			Components[1].Value = value.Y;
		}
	}
}

public class Vector3Variable<T> : VectorVariable<T, Vector3<T>> where T : unmanaged, INumber<T> {
	public Vector3Variable ( IRuntimeType<Vector3<T>> type, IRuntimeType<T> componentType ) : base( type, componentType, 3 ) { }

	public override Vector3<T> Value {
		get => new( Components[0].Value, Components[1].Value, Components[2].Value );
		set {
			Components[0].Value = value.X;
			Components[1].Value = value.Y;
			Components[2].Value = value.Z;
		}
	}
}

public class Vector4Variable<T> : VectorVariable<T, Vector4<T>> where T : unmanaged, INumber<T> {
	public Vector4Variable ( IRuntimeType<Vector4<T>> type, IRuntimeType<T> componentType ) : base( type, componentType, 4 ) { }

	public override Vector4<T> Value {
		get => new( Components[0].Value, Components[1].Value, Components[2].Value, Components[3].Value );
		set {
			Components[0].Value = value.X;
			Components[1].Value = value.Y;
			Components[2].Value = value.Z;
			Components[3].Value = value.W;
		}
	}
}

public class ArrayVariable : ICompositeVariable {
	public RuntimeArrayType Type;
	public ArrayVariable ( RuntimeArrayType type ) {
		Type = type;
		Elements = Enumerable.Range( 0, Type.Count ).Select( _ => Type.ElementType.CreateVariable() ).ToArray();
	}

	public IVariable[] Elements;
	IRuntimeType IVariable.Type => Type;
	public object? Value {
		get => Elements;
		set {
			throw new NotImplementedException();
		}
	}

	public void Parse ( ReadOnlySpan<byte> data ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"[{string.Join(", ", Elements.AsEnumerable())}]";
	}

	public IVariable this[uint index] => Elements[index];

	public void Interpolate ( float a, float b, float c, IVariable A, IVariable B, IVariable C ) {
		throw new NotImplementedException();
	}
}

public class PointerVariable : IVariable {
	public RuntimePointerType Type;
	public PointerVariable ( RuntimePointerType type ) {
		Type = type;
	}

	public IVariable? Address;
	IRuntimeType IVariable.Type => Type;
	public object? Value {
		get => Address;
		set {
			throw new NotImplementedException();
		}
	}

	public void Parse ( ReadOnlySpan<byte> data ) {
		Address!.Parse( data );
	}

	public override string ToString () {
		return Address is null ? "NULL" : $"&{Address}";
	}

	public void Interpolate ( float a, float b, float c, IVariable A, IVariable B, IVariable C ) {
		throw new NotImplementedException();
	}
}