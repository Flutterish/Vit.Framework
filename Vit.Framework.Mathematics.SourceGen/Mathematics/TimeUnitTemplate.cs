using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class TimeUnits : List<TimeUnitTemplate> {

}

public class TimeUnitTemplate : ClassTemplate<TimeUnits> {
	public readonly TimeSpan Span;
	public readonly string Name;

	public TimeUnitTemplate ( TimeSpan span, string name ) {
		Span = span;
		Name = name;
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
		sb.AppendLine( $"public static readonly TimeSpan UnitSpan = new TimeSpan( (long){Span.Ticks} );" );

		sb.AppendLine();
		sb.AppendLine( $"public static implicit operator TimeSpan ( {type} {Name.ToLower()} )" );
		sb.AppendLine( $"\t=> UnitSpan * {Name.ToLower()}.Value;" );

		sb.AppendLine();
		sb.AppendLine( "public override string ToString () {" );
		using ( sb.Indent() ) {
			GenerateToString( data, sb );
		}
		sb.AppendLine( "}" );
	}

	public virtual void GenerateToString ( TimeUnits data, SourceStringBuilder sb ) {
		sb.AppendLine( $"return $\"{{Value}} {Name.ToLower()}\";" );
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
			sb.AppendLine( "public override string ToString () {" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"return $\"{{Value}} per {Name.ToLower()[..^1]}\";" );
			}
			sb.AppendLine( "}" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public static class {GetTypeName( data )}Extensions {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"public static {GetFullTypeName( data )} {GetFullTypeName( data )} ( this double value )" );
			sb.AppendLine( "\t=> new( value );" );
		}
		sb.AppendLine( "}" );
	}
}