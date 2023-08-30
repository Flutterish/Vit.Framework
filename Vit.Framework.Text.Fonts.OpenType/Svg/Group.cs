namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public class Group : SvgElement {
	static HeapByteString transform = "transform";
	public override void Open ( ref SvgOutline.Context context ) {
		base.Open( ref context );
	}

	public override bool SetAttribute ( ref SvgOutline.Context context, ByteString name, ByteString unescapedValue ) {
		if ( base.SetAttribute( ref context, name, unescapedValue ) )
			return true;

		if ( name == transform ) {
			context.Matrix *= Transform.Parse( unescapedValue );
		}
		//else {
		//	return false;
		//}
		return true;
	}
}
