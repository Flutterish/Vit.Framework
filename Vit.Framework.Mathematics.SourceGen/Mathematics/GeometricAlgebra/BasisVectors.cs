using System.Collections.Immutable;
using Vit.Framework.Mathematics.GeometricAlgebra.Generic;

namespace Vit.Framework.Mathematics.SourceGen.Mathematics.GeometricAlgebra;

public static class BasisVectors {
	public static readonly MultiVector<float> a0 = new BasisVector<float>( "a₀" );
	public static readonly MultiVector<float> a1 = new BasisVector<float>( "a₁" );
	public static readonly MultiVector<float> a2 = new BasisVector<float>( "a₂" );
	public static readonly MultiVector<float> a3 = new BasisVector<float>( "a₃" );
	public static ImmutableArray<MultiVector<float>> ANames => (new[] { a0, a1, a2, a3 }).ToImmutableArray();

	public static readonly MultiVector<float> b0 = new BasisVector<float>( "b₀" );
	public static readonly MultiVector<float> b1 = new BasisVector<float>( "b₁" );
	public static readonly MultiVector<float> b2 = new BasisVector<float>( "b₂" );
	public static readonly MultiVector<float> b3 = new BasisVector<float>( "b₃" );
	public static ImmutableArray<MultiVector<float>> BNames => (new[] { b0, b1, b2, b3 }).ToImmutableArray();

	public static readonly BasisVector<MultiVector<float>> e0 = new( "X", 1 );
	public static readonly BasisVector<MultiVector<float>> e1 = new( "Y", 1 );
	public static readonly BasisVector<MultiVector<float>> e2 = new( "Z", 1 );
	public static readonly BasisVector<MultiVector<float>> e3 = new( "W", 1 );
	public static IReadOnlyList<BasisVector<MultiVector<float>>> Bases => new[] { e0, e1, e2, e3 };

	public static MultiVector<MultiVector<float>> MakeVector ( int dimensions, IReadOnlyList<MultiVector<float>> names ) {
		MultiVector<MultiVector<float>> vec = (MultiVector<float>)0;
		for ( int i = 0; i < dimensions; i++ ) {
			vec += Bases[i] * names[i];
		}

		return vec;
	}

	public static List<int[]> GenerateBasisIndices ( int dimensions, int count ) {
		List<int[]> values = new();
		Stack<int> used = new();
		
		void useNext ( int from ) {
			if ( used.Count == count ) {
				values.Add( used.Reverse().ToArray() );
				return;
			}

			for ( int i = from; i < dimensions; i++ ) {
				if ( used.Contains( i ) )
					continue;

				used.Push( i );
				useNext( i + 1 );
				used.Pop();
			}
		}

		useNext( 0 );
		return values;
	}
}
