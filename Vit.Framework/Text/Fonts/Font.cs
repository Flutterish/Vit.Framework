using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vit.Framework.Exceptions;

namespace Vit.Framework.Text.Fonts;

public abstract class Font {
	protected readonly Dictionary<GlyphId, Glyph> GlyphsById = new();
	protected readonly ByteCharPrefixTree<HashSet<Glyph>> GlyphsByVector = new();

	public string Name { get; protected set; } = string.Empty;
	public double UnitsPerEm { get; protected set; } = double.NaN;

	protected Glyph GetGlyph ( GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			GlyphsById.Add( id, glyph = new( id ) );

		return glyph;
	}

	public Glyph GetGlyph ( ReadOnlySpan<byte> glyphVector ) {
		if ( !GlyphsByVector.TryGetValue( glyphVector, out var set ) ) {
			TryLoadGlyphFor( glyphVector );
			if ( !GlyphsByVector.TryGetValue( glyphVector, out set ) ) {
				AddGlyphMapping( glyphVector, 0 );
				set = GlyphsByVector[glyphVector];
			}
		}

		Debug.Assert( set.Any() );
		return set.First();
	}
	public Glyph GetGlyph ( ReadOnlySpan<char> glyphVector ) {
		Span<byte> bytes = stackalloc byte[glyphVector.Length * 2 + 1];
		var length = Encoding.UTF8.GetBytes( glyphVector, bytes );
		return GetGlyph( bytes.Slice( 0, length ) );
	}
	public unsafe Glyph GetGlyph ( char character ) => GetGlyph( new Rune( character ) );
	public unsafe Glyph GetGlyph ( Rune rune ) {
		var value = rune.Value;
		return GetGlyph( new ReadOnlySpan<byte>( &value, rune.Utf8SequenceLength ) );
	}
	public Glyph? TryGetGlyph ( GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			return null;

		return glyph;
	}

	protected void AddGlyphMapping ( ReadOnlySpan<byte> glyphVector, GlyphId id ) {
		var glyph = GetGlyph( id );

		if ( !GlyphsByVector.TryGetValue( glyphVector, out var set ) )
			GlyphsByVector.Add( glyphVector, set = new() );

		if ( set.Add( glyph ) ) {
			glyph.AssignedVectors.Add( Encoding.UTF8.GetString( glyphVector ) );
		}
	}
	protected bool IsGlyphRegistered ( ReadOnlySpan<byte> glyphVector )
		=> GlyphsByVector.ContainsKey( glyphVector );

	protected abstract void TryLoadGlyphFor ( ReadOnlySpan<byte> glyphVector );

	public void Validate () {
		if ( double.IsNaN( UnitsPerEm ) )
			throw new InvalidStateException( "Font does not have `units per em` set" );
	}
}

public class ByteCharPrefixTree<TValue> {
	bool hasValue;
	TValue value = default!;
	ByteCharPrefixTree<TValue>?[]? children;

	public void Add ( ReadOnlySpan<byte> key, TValue value ) {
		var self = this;

		ref var node = ref self;
		foreach ( var i in key ) {
			if ( node.children == null )
				node.children = new ByteCharPrefixTree<TValue>[256];
			node = ref node.children[i];
			node ??= new();
		}

		node.hasValue = true;
		node.value = value;
	}

	public bool ContainsKey ( ReadOnlySpan<byte> key ) {
		var node = this;
		foreach ( var i in key ) {
			if ( node.children == null )
				return false;
			node = node.children[i];
			if ( node == null )
				return false;
		}

		return node.hasValue;
	}

	public bool TryGetValue ( ReadOnlySpan<byte> key, [NotNullWhen(true)] out TValue? value ) {
		var node = this;
		foreach ( var i in key ) {
			if ( node.children == null ) {
				value = default;
				return false;
			}

			node = node.children[i];
			if ( node == null ) {
				value = default;
				return false;
			}
		}

		value = node.value;
		return node.hasValue;
	}

	public TValue GetValue ( ReadOnlySpan<byte> key ) {
		var node = this;
		foreach ( var i in key ) {
			node = node.children![i]!;
		}

		return node.value;
	}

	public TValue this[ReadOnlySpan<byte> key] => GetValue( key );

	public override string ToString () {
		return $"{(hasValue ? (value is HashSet<Glyph> h ? h.FirstOrDefault() : value) : "")}{(children == null ? "" : "+")}";
	}
}