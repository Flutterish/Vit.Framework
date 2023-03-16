using Vit.Framework.SdlWindowing;

namespace Vit.Framework.Tests;

public class Program {
	public static void Main () {
		var host = new SdlHost();

		var a = host.CreateWindow();
		var b = host.CreateWindow();
		var c = host.CreateWindow();

		while ( !a.IsClosed || !b.IsClosed || !c.IsClosed ) {
			Thread.Sleep( 1 );
		}
		host.Dispose();
	}

	public Program () {
		
	}
}