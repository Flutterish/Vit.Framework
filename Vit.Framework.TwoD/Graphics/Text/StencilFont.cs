using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Outlines;

namespace Vit.Framework.TwoD.Graphics.Text;

public class StencilFontStore {
	Dictionary<Glyph, StencilGlyph> glyphs = new();

	public StencilGlyph GetGlyph ( Glyph glyph ) {
		if ( !glyphs.TryGetValue( glyph, out var stencil ) )
			glyphs.Add( glyph, stencil = new( glyph ) );

		return stencil;
	}
}

public class StencilGlyph {
	public readonly List<Point2<float>> Vertices = new();
	public readonly List<uint> Indices = new();

	public StencilGlyph ( Glyph glyph ) {
		if ( !glyph.TryFetchOutline<SplineOutline>( out var outline ) )
			return;

		load( outline );
	}

	public StencilGlyph ( SplineOutline outline ) {
		load( outline );
	}

	void load ( SplineOutline outline ) {
		foreach ( var spline in outline.Splines ) {
			uint? _anchor = null;
			uint? _last = null;
			foreach ( var p in spline.GetPoints() ) {
				var point = p.Cast<float>();
				var index = (uint)Vertices.Count;
				Vertices.Add( point );

				if ( _anchor is not uint anchor ) {
					_anchor = index;
					continue;
				}
				if ( _last is not uint last ) {
					_last = index;
					continue;
				}

				Indices.AddRange( new[] { anchor, last, index } );
				_last = index;
			}
		}
	}
}