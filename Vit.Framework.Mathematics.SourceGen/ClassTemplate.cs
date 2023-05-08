namespace Vit.Framework.Mathematics.SourceGen;

public abstract class ClassTemplate<T> : Template<T> {
	public abstract string GetTypeName ( T data );
	public abstract string GetFullTypeName ( T data );

	protected abstract void GenerateUsings ( T data, SourceStringBuilder sb );
	protected abstract string Namespace { get; }
	protected abstract string ClassType { get; }

	protected abstract void GenerateInterfaces ( T data, SourceStringBuilder sb );

	protected override string GetFileName ( T data ) {
		return $"{GetTypeName(data)}.cs";
	}

	protected sealed override void Generate ( T data, SourceStringBuilder sb ) {
		var type = GetFullTypeName( data );

		GenerateUsings( data, sb );
		sb.AppendLine();
		sb.AppendLine( $"namespace {Namespace};" );
		sb.AppendLine();
		sb.Append( $"public {ClassType} {type}" );
		GenerateInterfaces( data, sb );
		sb.AppendLine( " {" );
		using ( sb.Indent() )
			GenerateClassBody( data, sb );
		sb.AppendLine( "}" );

		GenerateAfter( data, sb );
	}

	protected abstract void GenerateClassBody ( T data, SourceStringBuilder sb );

	protected virtual void GenerateAfter ( T data, SourceStringBuilder sb ) { }
}
