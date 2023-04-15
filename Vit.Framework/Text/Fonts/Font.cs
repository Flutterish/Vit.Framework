﻿using System.Text;
using Vit.Framework.Exceptions;

namespace Vit.Framework.Text.Fonts;

public class Font {
	protected Dictionary<GlyphId, Glyph> GlyphsById = new();
	protected Dictionary<Rune, HashSet<Glyph>> GlyphsByRune = new();

	public string Name { get; protected set; } = string.Empty;
	public double UnitsPerEm { get; protected set; } = double.NaN;

	protected Glyph GetGlyph ( GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			GlyphsById.Add( id, glyph = new( id ) );

		return glyph;
	}

	public Glyph? TryGetGlyph ( char character ) => TryGetGlyph( new Rune(character) );
	public Glyph? TryGetGlyph ( Rune rune ) {
		if ( !GlyphsByRune.TryGetValue( rune, out var set ) )
			return null;

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

	public void Validate () {
		if ( double.IsNaN( UnitsPerEm ) )
			throw new InvalidStateException( "Font does not have `units per em` set" );
	}
}
