using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Mathematics.GeometricAlgebra.Generic;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Platform;

namespace Vit.Framework.Tests;

public class Program : App {
	public static void Main () {
		var x = new BasisVector<float>( "x̂", square: 1 );
		var y = new BasisVector<float>( "ŷ", square: 1 );
		var z = new BasisVector<float>( "ẑ", square: 1 );
		var w = new BasisVector<float>( "ŵ", square: 1 );
		var e0 = new BasisVector<float>( "e₀", square: 0 );

		var matA = new Matrix<MultiVector<float>>( new MultiVector<float>[,] {
			{  1,  2,  3, e0 },
			{  4,  5,  6, e0 },
			{  7,  8,  9, e0 },
			{ e0, e0, e0, e0 }
		} );
		var matB = (Matrix<MultiVector<float>>)new Vector<MultiVector<float>>( x, y, z, w );

		var res = matA * matB;
		var res2 = matB * matA;
		//using var host = new SdlHost();
		//host.Run( new Program() );
	}

	public override void Initialize ( Host host ) {
		var a = host.CreateWindow( RenderingApi.OpenGl );
		a.Title = "Window A";
		var b = host.CreateWindow( RenderingApi.OpenGl );
		b.Title = "Window B";
		var c = host.CreateWindow( RenderingApi.OpenGl );
		c.Title = "Window C";

		Task.Run( async () => {
			while ( !a.IsClosed || !b.IsClosed || !c.IsClosed )
				await Task.Delay( 1 );

			host.Quit();
		} );
	}
}