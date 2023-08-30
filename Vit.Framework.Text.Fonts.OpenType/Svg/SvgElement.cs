using System.Runtime.CompilerServices;

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
