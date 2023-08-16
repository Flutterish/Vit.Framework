namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class TimeUnits : List<TimeUnitTemplate> {

}

public class TimeUnitTemplate : ClassTemplate<TimeUnits> {
	public readonly TimeSpan Span;
	public readonly string Name;
	public readonly string Unit;

	public TimeUnitTemplate ( TimeSpan span, string name, string unit ) {
		Span = span;
		Name = name;
		Unit = unit;
	}

	public override string GetTypeName ( TimeUnits data ) {
		return $"{Name}";
	}

	public override string GetFullTypeName ( TimeUnits data ) {
		return GetTypeName( data );
	}

	protected override void GenerateUsings ( TimeUnits data, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
	}

	protected override string Namespace => "Vit.Framework.Mathematics";
	protected override string ClassType => "struct";

	protected override void GenerateInterfaces ( TimeUnits data, SourceStringBuilder sb ) {
		sb.Append( $" : IInterpolatable<{GetFullTypeName( data )}, double>" );
	}

	protected override void GenerateClassBody ( TimeUnits data, SourceStringBuilder sb ) {
		var nonGenericType = GetTypeName( data );
		var type = GetFullTypeName( data );

		sb.AppendLine( "public double Value;" );

		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType} ( double value ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "Value = value;" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {type} Lerp ( {type} goal, double time ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return new( Value.Lerp( goal.Value, time ) );" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public static readonly TimeSpan UnitSpan = new TimeSpan( {Span.Ticks}L );" );

		sb.AppendLine();
		sb.AppendLine( $"public static implicit operator TimeSpan ( {type} {Name.ToLower()} )" );
		sb.AppendLine( $"\t=> UnitSpan * {Name.ToLower()}.Value;" );

		sb.AppendLine();
		sb.AppendLine( $"public static implicit operator {type} ( TimeSpan time )" );
		sb.AppendLine( $"\t=> new( (double)time.Ticks / {Span.Ticks}L );" );

		generateOperator( sb, "+" );
		generateOperator( sb, "-" );
		generateOperator( sb, "/", "double" );
		generateOperator( sb, "==", "bool" );
		generateOperator( sb, "!=", "bool" );
		generateOperator( sb, ">", "bool" );
		generateOperator( sb, "<", "bool" );
		generateOperator( sb, ">=", "bool" );
		generateOperator( sb, "<=", "bool" );

		sb.AppendLine();
		sb.AppendLine( "public override string ToString () {" );
		using ( sb.Indent() ) {
			GenerateToString( data, sb );
		}
		sb.AppendLine( "}" );
	}

	public virtual void GenerateToString ( TimeUnits data, SourceStringBuilder sb ) {
		sb.AppendLine( $"return $\"{{Value}}{Unit}\";" );
	}

	void generateOperator ( SourceStringBuilder sb, string @operator, string? type = null ) {
		sb.AppendLine();
		sb.AppendLine( $"public static {(type ?? Name)} operator {@operator} ( {Name} left, {Name} right )" );
		using (sb.Indent() ) {
			if ( type != null )
				sb.AppendLine( $"=> left.Value {@operator} right.Value;" );
			else
				sb.AppendLine( $"=> new( left.Value {@operator} right.Value );" );
		}
	}

	protected override void GenerateAfter ( TimeUnits data, SourceStringBuilder sb ) {
		var nonGenericType = $"Per{Name[..^1]}";
		var type = $"Per{Name[..^1]}<T>";

		sb.AppendLine();
		sb.AppendLine( $"public struct {type} where T : IMultiplyOperators<T, double, T> {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "public T Value;" );

			sb.AppendLine();
			sb.AppendLine( $"public {nonGenericType} ( T value ) {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "Value = value;" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static T operator * ( {type} per, {GetFullTypeName( data )} time )" );
			sb.AppendLine( "\t=> per.Value * time.Value;" );

			sb.AppendLine();
			sb.AppendLine( $"public static T operator * ( {GetFullTypeName( data )} time, {type} per )" );
			sb.AppendLine( "\t=> per.Value * time.Value;" );

			sb.AppendLine();
			sb.AppendLine( "public override string ToString () {" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"return $\"{{Value}} per {Unit}\";" );
			}
			sb.AppendLine( "}" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public static class {GetTypeName( data )}Extensions {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"public static {GetFullTypeName( data )} {GetFullTypeName( data )} ( this double value )" );
			sb.AppendLine( "\t=> new( value );" );

			sb.AppendLine();
			sb.AppendLine( $"public static {GetFullTypeName( data )} {GetFullTypeName( data )} ( this int value )" );
			sb.AppendLine( "\t=> new( value );" );

			sb.AppendLine();
			sb.AppendLine( $"public static {type} {type} ( this T value ) where T : IMultiplyOperators<T, double, T>" );
			sb.AppendLine( "\t=> new( value );" );
		}
		sb.AppendLine( "}" );
	}
}