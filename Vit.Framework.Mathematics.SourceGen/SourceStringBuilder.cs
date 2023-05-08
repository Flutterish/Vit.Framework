using System.Text;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.SourceGen;

public class SourceStringBuilder {
	private StringBuilder builder = new();
	private int tabs = 0;

	public SourceStringBuilder Append ( string data ) {
		if ( newLine ) {
			newLine = false;
			builder.Append( new string( '\t', tabs ) );
		}
		builder.Append( data );
		return this;
	}

	bool newLine = true;
	public SourceStringBuilder AppendLine ( string data ) {
		if ( newLine ) {
			newLine = false;
			builder.Append( new string( '\t', tabs ) );
		}
		builder.AppendLine( data );
		newLine = true;
		return this;
	}

	public SourceStringBuilder AppendLine () {
		if ( newLine ) {
			newLine = false;
			builder.Append( new string( '\t', tabs ) );
		}
		builder.AppendLine();
		newLine = true;
		return this;
	}

	public SourceStringBuilder AppendJoin<T> ( string separator, IEnumerable<T> values ) {
		if ( newLine ) {
			newLine = false;
			builder.Append( new string( '\t', tabs ) );
		}
		builder.AppendJoin( separator, values );
		return this;
	}

	public SourceStringBuilder AppendLinePreJoin<T> ( string separator, IEnumerable<T> values ) {
		if ( newLine ) {
			newLine = false;
			builder.Append( new string( '\t', tabs ) );
		}
		builder.AppendJoin( Environment.NewLine + new string( '\t', tabs ) + separator, values );
		return this;
	}

	public SourceStringBuilder AppendLinePostJoin<T> ( string separator, IEnumerable<T> values ) {
		if ( newLine ) {
			newLine = false;
			builder.Append( new string( '\t', tabs ) );
		}
		builder.AppendJoin( separator + Environment.NewLine + new string( '\t', tabs ), values );
		return this;
	}

	public DisposeAction<SourceStringBuilder> Indent () {
		tabs++;
		return new( this, static self => self.tabs-- );
	}

	public override string ToString () {
		return builder.ToString();
	}
}
