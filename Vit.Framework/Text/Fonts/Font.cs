﻿using System.Diagnostics;
using System.Text;
using Vit.Framework.Exceptions;

namespace Vit.Framework.Text.Fonts;

public abstract class Font {
	protected Dictionary<GlyphId, Glyph> GlyphsById = new();
	protected Dictionary<Rune, HashSet<Glyph>> GlyphsByRune = new();

	public string Name { get; protected set; } = string.Empty;
	public double UnitsPerEm { get; protected set; } = double.NaN;

	protected Glyph GetGlyph ( GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			GlyphsById.Add( id, glyph = new( id ) );

		return glyph;
	}

	public Glyph GetGlyph ( ReadOnlyMemory<char> glyphVector ) { // TODO this should be the actual selector, not rune
		if ( glyphVector.Length == 1 ) {
			return GetGlyph( glyphVector.Span[0] );
		}
		else if ( glyphVector.Length == 2 ) {
			return GetGlyph( new Rune( glyphVector.Span[0], glyphVector.Span[1] ) );
		}
		else {
			return GetGlyph( '\0' );
		}
	}
	public Glyph GetGlyph ( char character ) => GetGlyph( new Rune(character) );
	public Glyph GetGlyph ( Rune rune ) {
		if ( !GlyphsByRune.TryGetValue( rune, out var set ) ) {
			TryLoadGlyphFor( rune );
			if ( !GlyphsByRune.TryGetValue( rune, out set ) ) {
				AddGlyphMapping( rune, 0 );
				set = GlyphsByRune[rune];
			}
		}

		Debug.Assert( set.Any() );
		return set.First();
	}
	public Glyph? TryGetGlyph ( GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			return null;

		return glyph;
	}

	protected void AddGlyphMapping ( Rune rune, GlyphId id ) {
		var glyph = GetGlyph( id );

		if ( !GlyphsByRune.TryGetValue( rune, out var set ) )
			GlyphsByRune.Add( rune, set = new() );

		if ( set.Add( glyph ) ) {
			glyph.AssignedRunes.Add( rune );
		}
	}
	protected bool IsRuneRegistered ( Rune rune )
		=> GlyphsByRune.ContainsKey( rune );

	protected abstract void TryLoadGlyphFor ( Rune rune );

	public void Validate () {
		if ( double.IsNaN( UnitsPerEm ) )
			throw new InvalidStateException( "Font does not have `units per em` set" );
	}
}
