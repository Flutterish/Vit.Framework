namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public class SvgRoot : SvgElement {
	static readonly HeapByteString xmlns = "xmlns";
	static readonly HeapByteString xmlnsValue = "http://www.w3.org/2000/svg";

	bool isXmlnsSet;
	public override void Open ( ref SvgOutline.Context context ) {
		base.Open( ref context );
		Assert( context.Depth == 0 );
	}

	public override bool SetAttribute ( ref SvgOutline.Context context, ByteString name, ByteString unescapedValue ) {
		if ( base.SetAttribute( ref context, name, unescapedValue ) )
			return true;

		if ( name == xmlns ) {
			isXmlnsSet = true;
			Assert( unescapedValue == xmlnsValue );
		}

		return true;
	}

	public override void Close ( ref SvgOutline.Context context ) {
		base.Close( ref context );
		Assert( isXmlnsSet );
	}
}
