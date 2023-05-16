using System.Diagnostics;
using Vit.Framework.Mathematics.GeometricAlgebra.Generic;

namespace Vit.Framework.Mathematics.SourceGen.Mathematics.GeometricAlgebra;

public class BiVectorTemplate : ClassTemplate<int> {
	protected virtual VectorTemplate CreateVectorTemplate () => new() { Path = string.Empty };
	VectorTemplate? _vector;
	VectorTemplate vector => _vector ??= CreateVectorTemplate();

	public override string GetTypeName ( int dim ) {
		return $"BiVector{dim}";
	}

	public override string GetFullTypeName ( int dim ) {
		return $"{GetTypeName(dim)}<T>";
	}

	protected override void GenerateUsings ( int data, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
	}

	protected override string Namespace => "Vit.Framework.Mathematics.GeometricAlgebra";
	protected override string ClassType => "struct";

	protected override void GenerateInterfaces ( int data, SourceStringBuilder sb ) {
		sb.Append( " where T : INumber<T>" );
	}

	protected override void GenerateClassBody ( int dim, SourceStringBuilder sb ) {
		foreach ( var i in BasisVectors.GenerateBasisIndices( dim, 2 ) ) {
			sb.AppendLine( $"public T {string.Join( "", i.Select( x => vector.AxisNames[x] ))};" );
		}
	}

	public void ScaleToString ( MultiVector<float> value, SourceStringBuilder sb, bool multiline, string aName = "A", string bName = "B" ) {
		bool first = true;
		foreach ( var i in value.Components ) {
			Debug.Assert( i.Scale is -1 or 1 );

			if ( first ) {
				if ( i.Scale < 0 )
					sb.Append( "-" );
			}
			else if ( i.Scale < 0 ) {
				if ( multiline ) {
					sb.AppendLine();
					sb.Append( "\t- " );
				}
				else {
					sb.Append( " - " );
				}
			}
			else {
				if ( multiline ) {
					sb.AppendLine();
					sb.Append( "\t+ " );
				}
				else {
					sb.Append( " + " );
				}
			}
			first = false;

			sb.AppendJoin( " * ", i.Bases.Select( x => {
				return $"{(x.Name[0] == 'a' ? aName : bName)}.{vector.AxisNames[x.Name[1] switch {
					'₀' => 0,
					'₁' => 1,
					'₂' => 2,
					'₃' or _ => 3
				}]}";
			} ) );
		}
	}
}