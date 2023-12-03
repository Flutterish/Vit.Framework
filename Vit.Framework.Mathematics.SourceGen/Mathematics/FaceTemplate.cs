using Vit.Framework.Mathematics.SourceGen.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class FaceTemplate : ClassTemplate<(int points, int size)> {
	PointTemplate? pointTemplate;
	PointTemplate PointTemplate => pointTemplate ??= new PointTemplate() { Path = "" };

	MatrixTemplate? matrixTemplate;
	MatrixTemplate MatrixTemplate => matrixTemplate ??= new() { Path = "" };

	AxisAlignedBoxTemplate? axisAlignedBoxTemplate;
	AxisAlignedBoxTemplate AxisAlignedBoxTemplate => axisAlignedBoxTemplate ??= new() { Path = "" };

	static readonly string[] Names = new[] { "Line", "Triangle", "Quad" };

	public static string GetPointName ( int points, int index ) {
		if ( points == 2 )
			return index == 0 ? "Start": "End";
		return $"Point{"ABCD"[index]}";
	}

	protected override string Namespace => "Vit.Framework.Mathematics";
	protected override string ClassType => "struct";

	public override string GetTypeName ( (int points, int size) _ ) {
		return $"{Names[_.points - 2]}{_.size}";
	}

	public override string GetFullTypeName ( (int points, int size) _ ) {
		return $"{GetTypeName(_)}<T>";
	}

	protected override void GenerateUsings ( (int points, int size) _, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
		sb.AppendLine( "using Vit.Framework.Mathematics.LinearAlgebra;" );
	}

	protected override void GenerateInterfaces ( (int points, int size) _, SourceStringBuilder sb ) {
		sb.Append( " where T : INumber<T>" );
	}

	protected override void GenerateClassBody ( (int points, int size) _, SourceStringBuilder sb ) {
		for ( int i = 0; i < _.points; i++ ) {
			sb.AppendLine( $"public {PointTemplate.GetFullTypeName(_.size)} {GetPointName(_.points, i)};" );
		}

		sb.AppendLine();
		var name = Names[_.size - 2].ToLowerInvariant();
		sb.AppendLine( $"public static {GetFullTypeName( _ )} operator * ( {GetFullTypeName( _ )} {name}, {MatrixTemplate.GetFullTypeName( (_.size, _.size) )} matrix ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() ) {
				sb.AppendLinePostJoin( ",", Enumerable.Range( 0, _.points ).Select( i => $"{GetPointName( _.points, i )} = {name}.{GetPointName( _.points, i )} * matrix" ) );
			}
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		if ( _.size < 4 ) { // NOTE we generate up to mat4
			sb.AppendLine();
			sb.AppendLine( $"public static {GetFullTypeName( _ )} operator * ( {GetFullTypeName( _ )} {name}, {MatrixTemplate.GetFullTypeName( (_.size + 1, _.size + 1) )} matrix ) {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					sb.AppendLinePostJoin( ",", Enumerable.Range( 0, _.points ).Select( i => $"{GetPointName( _.points, i )} = matrix.Apply( {name}.{GetPointName( _.points, i )} )" ) );
				}
				sb.AppendLine();
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}

		sb.AppendLine();
		sb.AppendLine( $"public readonly {AxisAlignedBoxTemplate.GetFullTypeName(_.size)} BoundingBox => new() {{" );
		using ( sb.Indent() ) {
			sb.AppendLinePostJoin( ",", Enumerable.Range( 0, _.size ).SelectMany( i => new[] {
				$"Min{PointTemplate.AxisNames[i]} = {Enumerable.Range(0, _.points - 2).Select( x => $"{GetPointName(_.points, x)}.{PointTemplate.AxisNames[i]}" ).Aggregate( $"T.Min( {GetPointName(_.points, _.points - 2)}.{PointTemplate.AxisNames[i]}, {GetPointName(_.points, _.points - 1)}.{PointTemplate.AxisNames[i]} )", (a, b) => $"T.Min( {a}, {b} )" )}",
				$"Max{PointTemplate.AxisNames[i]} = {Enumerable.Range(0, _.points - 2).Select( x => $"{GetPointName(_.points, x)}.{PointTemplate.AxisNames[i]}" ).Aggregate( $"T.Max( {GetPointName(_.points, _.points - 2)}.{PointTemplate.AxisNames[i]}, {GetPointName(_.points, _.points - 1)}.{PointTemplate.AxisNames[i]} )", (a, b) => $"T.Max( {a}, {b} )" )}"
			} ) );
			sb.AppendLine();
		}
		sb.AppendLine( "};" );
	}
}
