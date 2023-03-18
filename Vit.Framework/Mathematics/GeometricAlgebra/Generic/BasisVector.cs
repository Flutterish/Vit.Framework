using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics.GeometricAlgebra.Generic;

public class BasisVector<T> where T : INumber<T> {
	public readonly string Name;
	public readonly T Square;
	public readonly bool CanSquare;
	public readonly int SortingOrder;

	static int order;

	public BasisVector ( string name, T square ) {
		Name = name;
		Square = square;
		CanSquare = true;
		SortingOrder = order++;
	}

	public BasisVector ( string name ) {
		Name = name;
		Square = T.MultiplicativeIdentity;
		SortingOrder = order++;
	}

	public static SimpleBlade<T> operator * ( BasisVector<T> left, BasisVector<T> right ) {
		return (SimpleBlade<T>)left * (SimpleBlade<T>)right;
	}

	public static MultiVector<T> operator * ( MultiVector<T> left, BasisVector<T> right ) {
		return left * (SimpleBlade<T>)right;
	}
	public static MultiVector<T> operator * ( BasisVector<T> left, MultiVector<T> right ) {
		return (SimpleBlade<T>)left * right;
	}

	public static MultiVector<T> operator + ( MultiVector<T> left, BasisVector<T> right ) {
		return left + (MultiVector<T>)right;
	}
	public static MultiVector<T> operator + ( BasisVector<T> left, MultiVector<T> right ) {
		return (MultiVector<T>)left + right;
	}
	public static MultiVector<T> operator + ( BasisVector<T> left, BasisVector<T> right ) {
		return (MultiVector<T>)left + (MultiVector<T>)right;
	}

	public static MultiVector<T> operator - ( MultiVector<T> left, BasisVector<T> right ) {
		return left - (MultiVector<T>)right;
	}
	public static MultiVector<T> operator - ( BasisVector<T> left, MultiVector<T> right ) {
		return (MultiVector<T>)left - right;
	}
	public static MultiVector<T> operator - ( BasisVector<T> left, BasisVector<T> right ) {
		return (MultiVector<T>)left - (MultiVector<T>)right;
	}

	public static implicit operator SimpleBlade<T> ( BasisVector<T> basis )
		=> new( T.One, MemoryMarshal.CreateReadOnlySpan( ref basis, 1 ) );

	public static implicit operator MultiVector<T> ( BasisVector<T> basis ) {
		var blade = (SimpleBlade<T>)basis;
		return new MultiVector<T>( MemoryMarshal.CreateReadOnlySpan( ref blade, 1 ) );
	}

	public override string ToString () {
		return Name;
	}
}
