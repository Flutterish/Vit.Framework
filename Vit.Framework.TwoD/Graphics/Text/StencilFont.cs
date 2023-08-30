using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;

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
		foreach ( var spline in glyph.Outline.Splines ) {
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