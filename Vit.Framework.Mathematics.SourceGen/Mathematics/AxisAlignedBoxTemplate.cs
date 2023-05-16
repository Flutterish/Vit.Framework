namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class AxisAlignedBoxTemplate : ClassTemplate<int> {
	protected override string Namespace => "Vit.Framework.Mathematics";

	PointTemplate pointType = new() { Path = string.Empty };
	SizeTemplate sizeType = new() { Path = string.Empty };

	public override string GetTypeName ( int size ) {
		return $"AxisAlignedBox{size}";
	}

	public override string GetFullTypeName ( int size ) {
		return $"AxisAlignedBox{size}<T>";
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
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
		sb.AppendLine( $"public static implicit operator {type} ( {sizeType.GetFullTypeName( size )} size ) => new( size );" );

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
