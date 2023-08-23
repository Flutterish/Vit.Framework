using Vit.Framework.Graphics.Software.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Curses;

public class CursesRenderer : SoftwareRenderer {
	public CursesRenderer ( CursesApi graphicsApi ) : base( graphicsApi ) { }

	public override Matrix4<T> CreateNdcCorrectionMatrix<T> () {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}

	protected override void Dispose ( bool disposing ) {
		
	}
}
