using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public abstract class SvgElement {
	public virtual void Open ( ref SvgOutline.Context context ) {

	}
	public virtual bool SetAttribute ( ref SvgOutline.Context context, ByteString name, ByteString unescapedValue ) {
		return false;
	}
	public virtual void Close ( ref SvgOutline.Context context ) {

	}

	protected static void Assert ( bool value, [CallerArgumentExpression( nameof( value ) )] string expr = null! ) {
		if ( !value )
			throw new InvalidDataException( $"Expected {expr} to be true, but it was not." );
	}
}
