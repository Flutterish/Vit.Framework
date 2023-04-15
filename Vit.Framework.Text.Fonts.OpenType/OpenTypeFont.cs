using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Memory;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary.Views;

namespace Vit.Framework.Text.Fonts.OpenType;

public class OpenTypeFont : Font {
	Ref<Stream> streamRef = new( null! );
	ReopenableStream source;

	OpenFontFile header;

	public OpenTypeFont ( ReopenableStream source ) {
		this.source = source;
		
		bool wasOpen = source.IsOpen;
		header = BinaryView<OpenFontFile>.Parse( source.Open() );

		if ( !wasOpen )
			source.Close();
	}
}
