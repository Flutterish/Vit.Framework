using Vit.Framework.Mathematics.SourceGen.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class AxisAlignedBoxTemplate : ClassTemplate<int> {
	protected override string Namespace => "Vit.Framework.Mathematics";

	PointTemplate pointType = new() { Path = string.Empty };
	VectorTemplate vectorType = new() { Path = string.Empty };
	SizeTemplate sizeType = new() { Path = string.Empty };

	FaceTemplate? faceTemplate;
	FaceTemplate FaceTemplate => faceTemplate ??= new FaceTemplate() { Path = "" };

	MatrixTemplate? matrixTemplate;
	MatrixTemplate MatrixTemplate => matrixTemplate ??= new MatrixTemplate() { Path = "" };

	static (string forward, string backward)[] Directions = new[] {
		("Right", "Left"),
		("Top", "Bottom"),
		("Forward", "Backward"),
		("Ana", "Kata")
	};
	static string GeneratePointName ( int index, int size ) {
		string result = "";
		for ( int i = size - 1; i >= 0; i-- ) {
			result += (index & (1 << i)) == 0 ? Directions[i].forward : Directions[i].backward;
		}
		return result;
	}

	public override string GetTypeName ( int size ) {
		return $"AxisAlignedBox{size}";
	}

	public override string GetFullTypeName ( int size ) {
		return $"AxisAlignedBox{size}<T>";
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
		sb.AppendLine( "using Vit.Framework.Mathematics.LinearAlgebra;" );
	}

	protected override string ClassType => "struct";

	protected override void GenerateInterfaces ( int size, SourceStringBuilder sb ) {
		sb.Append( " where T : INumber<T>" );
	}

	protected override void GenerateClassBody ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		var nonGenericType = GetTypeName( size );
		var type = GetFullTypeName( size );

		foreach ( var i in elements ) {
			sb.AppendLine( $"public T Min{pointType.AxisNames[i]};" );
			sb.AppendLine( $"public T Max{pointType.AxisNames[i]};" );
		}

		sb.AppendLine();
		foreach ( var i in elements ) {
			sb.AppendLine( $"public T {sizeType.AxisNames[i]} => Max{pointType.AxisNames[i]} - Min{pointType.AxisNames[i]};" );
		}

		sb.AppendLine();
		sb.Append( $"public {pointType.GetFullTypeName( size )} Position => new( " );
		sb.AppendJoin( ", ", elements.Select( i => $"Min{pointType.AxisNames[i]}" ) );
		sb.AppendLine( " );" );
		sb.Append( $"public {sizeType.GetFullTypeName( size )} Size => new( " );
		sb.AppendJoin( ", ", elements.Select( i => $"{sizeType.AxisNames[i]}" ) );
		sb.AppendLine( " );" );

		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType} ( {sizeType.GetFullTypeName( size )} size ) {{" );
		using ( sb.Indent() ) {
			foreach ( var i in elements ) {
				sb.AppendLine( $"Min{pointType.AxisNames[i]} = T.Zero;" );
				sb.AppendLine( $"Max{pointType.AxisNames[i]} = size.{sizeType.AxisNames[i]};" );
			}
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType} ( {pointType.GetFullTypeName( size )} position, {sizeType.GetFullTypeName( size )} size ) {{" );
		using ( sb.Indent() ) {
			foreach ( var i in elements ) {
				sb.AppendLine( $"Min{pointType.AxisNames[i]} = position.{pointType.AxisNames[i]};" );
				sb.AppendLine( $"Max{pointType.AxisNames[i]} = Min{pointType.AxisNames[i]} + size.{sizeType.AxisNames[i]};" );
			}
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.Append( $"public {nonGenericType} ( " );
		sb.AppendJoin( ", ", elements.Select( x => "T " + pointType.AxisNames[x].ToLower() ) );
		sb.Append( ", " );
		sb.AppendJoin( ", ", elements.Select( x => "T " + sizeType.AxisNames[x].ToLower() ) );
		sb.AppendLine( " ) {" );
		using ( sb.Indent() ) {
			foreach ( var i in elements ) {
				sb.AppendLine( $"Min{pointType.AxisNames[i]} = {pointType.AxisNames[i].ToLower()};" );
				sb.AppendLine( $"Max{pointType.AxisNames[i]} = Min{pointType.AxisNames[i]} + {sizeType.AxisNames[i].ToLower()};" );
			}
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {type} Contain ( {type} other ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"Min{pointType.AxisNames[x]} = T.Min( Min{pointType.AxisNames[x]}, other.Min{pointType.AxisNames[x]} )" ) );
			sb.AppendLine( "," );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"Max{pointType.AxisNames[x]} = T.Max( Max{pointType.AxisNames[x]}, other.Max{pointType.AxisNames[x]} )" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {type} Intersect ( {type} other ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"Min{pointType.AxisNames[x]} = T.Max( Min{pointType.AxisNames[x]}, other.Min{pointType.AxisNames[x]} )" ) );
			sb.AppendLine( "," );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"Max{pointType.AxisNames[x]} = T.Min( Max{pointType.AxisNames[x]}, other.Max{pointType.AxisNames[x]} )" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public bool Contains ( {pointType.GetFullTypeName(size)} point )" );
		using ( sb.Indent() ) {
			sb.Append( "=> " );
			sb.AppendLinePreJoin( "&& ", elements.Select( x => $"Min{pointType.AxisNames[x]} <= point.{pointType.AxisNames[x]} && Max{pointType.AxisNames[x]} >= point.{pointType.AxisNames[x]}" ) );
			sb.AppendLine( ";" );
		}

		sb.AppendLine();
		sb.AppendLine( $"public bool IntersectsWith ( {GetFullTypeName( size )} other ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "var intersect = Intersect( other );" );
			sb.Append( "return " );
			using ( sb.Indent() ) {
				sb.AppendLinePreJoin( "&& ", elements.Select( x => $"intersect.{sizeType.AxisNames[x]} >= T.Zero" ) );
				sb.AppendLine( ";" );
			}
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public static implicit operator {type} ( {sizeType.GetFullTypeName( size )} size ) => new( size );" );

		var vectorTypeName = vectorType.GetFullTypeName( size );
		sb.AppendLine();
		sb.AppendLine( $"public static {type} operator + ( {type} left, {vectorTypeName} right ) => new() {{" );
		using ( sb.Indent() )
			sb.AppendLinePostJoin( ",", elements.Select( x => $"Min{pointType.AxisNames[x]} = left.Min{pointType.AxisNames[x]} + right.{vectorType.AxisNames[x]}" ) );
		sb.AppendLine( "," );
		using ( sb.Indent() )
			sb.AppendLinePostJoin( ",", elements.Select( x => $"Max{pointType.AxisNames[x]} = left.Max{pointType.AxisNames[x]} + right.{vectorType.AxisNames[x]}" ) );
		sb.AppendLine();
		sb.AppendLine( "};" );

		sb.AppendLine();
		sb.AppendLine( $"public static {type} operator - ( {type} left, {vectorTypeName} right ) => new() {{" );
		using ( sb.Indent() )
			sb.AppendLinePostJoin( ",", elements.Select( x => $"Min{pointType.AxisNames[x]} = left.Min{pointType.AxisNames[x]} - right.{vectorType.AxisNames[x]}" ) );
		sb.AppendLine( "," );
		using ( sb.Indent() )
			sb.AppendLinePostJoin( ",", elements.Select( x => $"Max{pointType.AxisNames[x]} = left.Max{pointType.AxisNames[x]} - right.{vectorType.AxisNames[x]}" ) );
		sb.AppendLine();
		sb.AppendLine( "};" );

		if ( size == 2 ) {
			sb.AppendLine();
			sb.AppendLine( $"public static implicit operator {FaceTemplate.GetFullTypeName( (4, size) )} ( {type} box ) => new() {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"{FaceTemplate.GetPointName( 4, 0 )} = new( box.Min{pointType.AxisNames[0]}, box.Min{pointType.AxisNames[1]} )," );
				sb.AppendLine( $"{FaceTemplate.GetPointName( 4, 1 )} = new( box.Max{pointType.AxisNames[0]}, box.Min{pointType.AxisNames[1]} )," );
				sb.AppendLine( $"{FaceTemplate.GetPointName( 4, 2 )} = new( box.Max{pointType.AxisNames[0]}, box.Max{pointType.AxisNames[1]} )," );
				sb.AppendLine( $"{FaceTemplate.GetPointName( 4, 3 )} = new( box.Min{pointType.AxisNames[0]}, box.Max{pointType.AxisNames[1]} )" );
			}
			sb.AppendLine( "};" );

			sb.AppendLine();
			sb.AppendLine( $"public static {FaceTemplate.GetFullTypeName( (4, size) )} operator * ( {type} box, {MatrixTemplate.GetFullTypeName( (3, 3) )} matrix )" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"=> (({FaceTemplate.GetFullTypeName( (4, size) )})box) * matrix;" );
			}

			sb.AppendLine();
			sb.AppendLine( $"public static {FaceTemplate.GetFullTypeName( (4, size) )} operator * ( {type} box, {MatrixTemplate.GetFullTypeName( (2, 2) )} matrix )" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"=> (({FaceTemplate.GetFullTypeName( (4, size) )})box) * matrix;" );
			}
		}

		sb.AppendLine();
		sb.AppendLine( "public override string ToString () {" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return $\"{Size} at {Position}\";" );
		}
		sb.AppendLine( "}" );
	}

	protected override void GenerateAfter ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );

		sb.AppendLine();
		sb.AppendLine( $"public static class AABox{size}<T> where T : INumber<T>, IFloatingPointIeee754<T> {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"public static {GetFullTypeName( size )} Undefined {{ get; }} = new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"Min{pointType.AxisNames[x]} = T.PositiveInfinity" ) );
			sb.AppendLine( "," );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"Max{pointType.AxisNames[x]} = T.NegativeInfinity" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}
}
