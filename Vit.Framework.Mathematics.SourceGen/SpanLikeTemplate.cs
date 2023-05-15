namespace Vit.Framework.Mathematics.SourceGen;

public abstract class SpanLikeTemplate : ClassTemplate<int> {
	protected override string ClassType => "struct";
	public override string GetFullTypeName ( int size ) {
		return $"{GetTypeName(size)}<T>";
	}
	protected override void GenerateInterfaces ( int size, SourceStringBuilder sb ) {
		var nonGenericType = GetTypeName( size );
		var type = $"{nonGenericType}<T>";
		sb.Append( $" : IInterpolatable<{type}, T>, IEqualityOperators<{type}, {type}, bool>, IEquatable<{type}>, IValueSpan<T> where T : INumber<T>" );
	}
	public IReadOnlyList<string> AxisNames { get; protected set; } = new[] { "X", "Y", "Z", "W" };

	protected override void GenerateClassBody ( int size, SourceStringBuilder sb ) {
		var nonGenericType = GetTypeName( size );
		var type = $"{nonGenericType}<T>";
		var elements = Enumerable.Range( 0, size );

		foreach ( var i in elements ) {
			sb.AppendLine( $"public T {AxisNames[i]};" );
		}

		sb.AppendLine();
		sb.Append( $"public {nonGenericType} ( " );
		sb.AppendJoin( ", ", elements.Select( x => "T " + AxisNames[x].ToLower() ) );
		sb.AppendLine( " ) {" );
		using ( sb.Indent() ) {
			foreach ( var i in elements ) {
				sb.AppendLine( $"{AxisNames[i]} = {AxisNames[i].ToLower()};" );
			}
		}
		sb.AppendLine( "}" );

		if ( size == 1 )
			goto endAllCtr;
		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType} ( T all ) {{" );
		using ( sb.Indent() ) {
			sb.AppendJoin( " = ", elements.Select( x => AxisNames[x] ) );
			sb.AppendLine( " = all;" );
		}
		sb.AppendLine( "}" );
	endAllCtr:

		sb.AppendLine();
		sb.AppendLine( "#nullable disable" );
		sb.AppendLine( $"public {nonGenericType} ( ReadOnlySpan<T> span ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "span.CopyTo( this.AsSpan() );" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType} ( IReadOnlyValueSpan<T> span ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "span.AsReadOnlySpan().CopyTo( this.AsSpan() );" );
		}
		sb.AppendLine( "}" );
		sb.AppendLine( "#nullable restore" );

		sb.AppendLine();
		foreach ( var i in elements ) {
			sb.Append( $"public static readonly {type} Unit{AxisNames[i]} = new( " );
			sb.AppendJoin( ", ", elements.Select( x => x == i ? "T.One" : "T.Zero" ) );
			sb.AppendLine( " );" );
		}
		sb.AppendLine( $"public static readonly {type} One = new( T.One );" );
		sb.AppendLine( $"public static readonly {type} Zero = new( T.Zero );" );

		generateSwizzles( size, sb );
		GenerateProperties( size, sb );

		sb.AppendLine();
		sb.AppendLine( $"public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref {AxisNames[0]}, {size} );" );
		sb.AppendLine( $"public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref {AxisNames[0]}, {size} );" );

		sb.AppendLine();
		sb.AppendLine( $"public {nonGenericType}<Y> Cast<Y> () where Y : INumber<Y> {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{AxisNames[x]} = Y.CreateChecked( {( AxisNames[x] == "Y" ? "this." : "" )}{AxisNames[x]} )" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		GenerateMethods( size, sb );
		sb.AppendLine();
		sb.AppendLine( $"public {type} Lerp ( {type} goal, T time ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{AxisNames[x]} = {AxisNames[x]}.Lerp( goal.{AxisNames[x]}, time )" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		GenerateOperators( size, sb );

		sb.AppendLine();
		sb.AppendLine( $"public static bool operator == ( {type} left, {type} right ) {{" );
		using ( sb.Indent() ) {
			sb.Append( "return " );
			using var _ = sb.Indent();
			sb.AppendLinePreJoin( "&& ", elements.Select( i => $"left.{AxisNames[i]} == right.{AxisNames[i]}" ) );
			sb.AppendLine( ";" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public static bool operator != ( {type} left, {type} right ) {{" );
		using ( sb.Indent() ) {
			sb.Append( "return " );
			using var _ = sb.Indent();
			sb.AppendLinePreJoin( "|| ", elements.Select( i => $"left.{AxisNames[i]} != right.{AxisNames[i]}" ) );
			sb.AppendLine( ";" );
		}
		sb.AppendLine( "}" );

		if ( size != 1 ) {
			sb.Append( $"public static implicit operator {type} ( (" );
			sb.AppendJoin( ", ", elements.Select( x => "T" ) );
			sb.AppendLine( ") value )" );
			sb.Append( "\t=> new( " );
			sb.AppendJoin( ", ", elements.Select( x => $"value.Item{x + 1}" ) );
			sb.AppendLine( " );" );
		}
		else {
			sb.Append( $"public static implicit operator {type} ( T value )" );
			sb.Append( "\t=> new( value );" );
		}

		sb.AppendLine();
		sb.Append( "public void Deconstruct ( " );
		sb.AppendJoin( ", ", elements.Select( x => $"out T {AxisNames[x].ToLower()}" ) );
		sb.AppendLine( " ) {" );
		using ( sb.Indent() ) {
			foreach ( var i in elements ) {
				sb.AppendLine( $"{AxisNames[i].ToLower()} = {AxisNames[i]};" );
			}
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( "public override bool Equals ( object? obj ) {" );
		using ( sb.Indent() )
			sb.AppendLine( $"return obj is {type} axes && Equals( axes );" );
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public bool Equals ( {type} other ) {{" );
		using ( sb.Indent() )
			sb.AppendLine( "return this == other;" );
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( "public override int GetHashCode () {" );
		using ( sb.Indent() ) {
			sb.Append( "return HashCode.Combine( " );
			sb.AppendJoin( ", ", elements.Select( x => AxisNames[x] ) );
			sb.AppendLine( " );" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( "public override string ToString () {" );
		using ( sb.Indent() )
			GenerateToString( size, sb );
		sb.AppendLine( "}" );
	}

	protected virtual void GenerateToString ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		sb.Append( "return $\"<" );
		sb.AppendJoin( ", ", elements.Select( x => $"{{{AxisNames[x]}}}" ) );
		sb.AppendLine( ">\";" );
	}

	protected void GenerateComponentwiseOperator ( int size, SourceStringBuilder sb, string @operator,
		SpanLikeTemplate? left = null, SpanLikeTemplate? right = null, SpanLikeTemplate? result = null,
		string leftName = "left", string rightName = "right"
	) {
		left ??= this;
		right ??= this;
		result ??= this;

		var elements = Enumerable.Range( 0, size );
		sb.AppendLine();
		sb.AppendLine( $"public static {result.GetTypeName( size )}<T> operator {@operator} ( {left.GetTypeName( size )}<T> {leftName}, {right.GetTypeName( size )}<T> {rightName} ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{result.AxisNames[x]} = {leftName}.{left.AxisNames[x]} {@operator} {rightName}.{right.AxisNames[x]}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	protected void GenerateComponentwiseScalarOperatorRight ( int size, SourceStringBuilder sb, string @operator,
		SpanLikeTemplate? left = null, SpanLikeTemplate? result = null,
		string leftName = "left", string rightName = "right"
	) {
		left ??= this;
		result ??= this;

		var elements = Enumerable.Range( 0, size );
		sb.AppendLine();
		sb.AppendLine( $"public static {result.GetTypeName( size )}<T> operator {@operator} ( {left.GetTypeName( size )}<T> {leftName}, T {rightName} ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{result.AxisNames[x]} = {leftName}.{left.AxisNames[x]} {@operator} {rightName}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	protected void GenerateComponentwiseScalarOperatorLeft ( int size, SourceStringBuilder sb, string @operator,
		SpanLikeTemplate? right = null, SpanLikeTemplate? result = null,
		string leftName = "left", string rightName = "right"
	) {
		right ??= this;
		result ??= this;

		var elements = Enumerable.Range( 0, size );
		sb.AppendLine();
		sb.AppendLine( $"public static {result.GetTypeName( size )}<T> operator {@operator} ( T {leftName}, {right.GetTypeName( size )}<T> {rightName} ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( "return new() {" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{result.AxisNames[x]} = {leftName} {@operator} {rightName}.{right.AxisNames[x]}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		sb.AppendLine( "using System.Numerics;" );
		sb.AppendLine( "using System.Runtime.InteropServices;" );
		sb.AppendLine( "using Vit.Framework.Memory;" );
	}

	protected virtual void GenerateProperties ( int size, SourceStringBuilder sb ) {

	}

	protected virtual void GenerateOperators ( int size, SourceStringBuilder sb ) {

	}

	protected virtual void GenerateMethods ( int size, SourceStringBuilder sb ) {

	}

	void generateSwizzles ( int size, SourceStringBuilder sb ) {
		Stack<int> usedIndices = new();
		void generateRest ( Stack<int> usedIndices ) {
			if ( usedIndices.Count > 1 && usedIndices.Count != size ) {
				sb.Append( $"public {GetFullTypeName(usedIndices.Count)} " );
				sb.AppendJoin( "", usedIndices.Select( x => AxisNames[x] ) );
				sb.Append( " => new( " );
				sb.AppendJoin( ", ", usedIndices.Select( x => AxisNames[x] ) );
				sb.AppendLine( " );" );
			}
			
			for ( int i = 0; i < size; i++ ) {
				if ( usedIndices.Contains( i ) )
					continue;

				usedIndices.Push( i );
				generateRest( usedIndices );
				usedIndices.Pop();
			}
		}

		sb.AppendLine();
		generateRest( usedIndices );
	}
}
