﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vit.Framework.Exceptions;
using Vit.Framework.Text.Outlines;

namespace Vit.Framework.Text.Fonts;

public abstract class Font {
	protected readonly Dictionary<GlyphId, Glyph> GlyphsById = new();
	protected readonly CodepointPrefixTree<Glyph> GlyphsByCluster = new();

	public string Name { get; protected set; } = string.Empty;
	public double UnitsPerEm { get; protected set; } = double.NaN;

	protected Glyph GetGlyph ( GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			GlyphsById.Add( id, glyph = new( id, this ) );

		return glyph;
	}

	public Glyph GetGlyph ( UnicodeExtendedGraphemeCluster cluster ) {
		if ( !GlyphsByCluster.TryGetValue( cluster, out var glyph ) ) {
			TryLoadGlyphFor( cluster );
			if ( !GlyphsByCluster.TryGetValue( cluster, out glyph ) ) {
				AddGlyphMapping( cluster, 0 );
				glyph = GlyphsByCluster[cluster];
			}
		}

		return glyph;
	}
	public unsafe Glyph GetGlyph ( ReadOnlySpan<char> graphemeCluster ) {
		var length = Encoding.UTF32.GetByteCount( graphemeCluster );
		byte* bytes = stackalloc byte[length];
		Encoding.UTF32.GetBytes( graphemeCluster, new Span<byte>( bytes, length ) );

		return GetGlyph( new UnicodeExtendedGraphemeCluster( bytes, length ) );
	}
	public Glyph? TryGetGlyph ( GlyphId id ) { // TODO try loading the glyph by id
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			return null;

		return glyph;
	}

	protected void AddGlyphMapping ( UnicodeExtendedGraphemeCluster cluster, GlyphId id ) {
		var glyph = GetGlyph( id );

		if ( GlyphsByCluster.TryGetValue( cluster, out var existing ) ) {
			if ( existing != glyph ) {
				throw new InvalidOperationException( "Can not map multiple glyph ids to the same glyph vector." );
			}
		}
		else {
			GlyphsByCluster.Add( cluster, glyph );

			var stringVector = cluster.ToString();
			glyph.AssignedVectors.Add( stringVector );
		}
	}
	protected bool IsGlyphRegistered ( UnicodeExtendedGraphemeCluster cluster )
		=> GlyphsByCluster.ContainsKey( cluster );

	protected abstract void TryLoadGlyphFor ( UnicodeExtendedGraphemeCluster cluster );
	public bool TryFetchOutline<TOutline> ( GlyphId id, [NotNullWhen(true)] out TOutline? outline ) where TOutline : IGlyphOutline {
		var outlines = FetchOutlines<TOutline>( id, 1 );
		if ( !outlines.Any() ) {
			outline = default;
			return false;
		}

		outline = outlines.Single().outline;
		return true;
	}
	public IEnumerable<(GlyphId id, TOutline outline)> FetchOutlines<TOutline> ( GlyphId firstId, int count ) where TOutline : IGlyphOutline {
		return FetchOutlines<TOutline>( Enumerable.Range( 0, count ).Select( x => new GlyphId( firstId.Value + (uint)x ) ) );
	}
	public abstract IEnumerable<(GlyphId id, TOutline outline)> FetchOutlines<TOutline> ( IEnumerable<GlyphId> ids ) where TOutline : IGlyphOutline;

	public void Validate () {
		if ( double.IsNaN( UnitsPerEm ) )
			throw new InvalidStateException( "Font does not have `units per em` set" );
	}
}
