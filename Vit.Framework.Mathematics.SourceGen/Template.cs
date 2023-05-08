namespace Vit.Framework.Mathematics.SourceGen;

public abstract class Template<T> {
	public required string Path { get; init; }

	protected abstract void Generate ( T data, SourceStringBuilder sb );
	protected abstract string GetFileName ( T data );

	public void Apply ( T data ) {
		var name = GetFileName( data );
		var source = Generate( data );
		File.WriteAllText( System.IO.Path.Combine( Path, name ), source );
		Console.WriteLine( $"Created {name} at {Path} ({source.Count( x => x == '\n' ) + 1} lines)" );
	}

	public string Generate ( T data ) {
		var name = GetFileName( data );
		SourceStringBuilder sb = new();
		sb.AppendLine( $"/// This file [{name}] was auto-generated with {GetType()} and parameter {data} ({typeof( T )})" );

		Generate( data, sb );
		return sb.ToString();
	}
}
