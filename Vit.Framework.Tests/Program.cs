using Vit.Framework.SdlWindowing;

namespace Vit.Framework.Tests;

public class Program {
	public static void Main () {
		var host = new SdlHost();

		var a = host.CreateWindow();
		a.Title = "Window A";
		var b = host.CreateWindow();
		b.Title = "Window B";
		var c = host.CreateWindow();
		c.Title = "Window C";

		while ( !a.IsClosed || !b.IsClosed || !c.IsClosed ) {
			Thread.Sleep( 1 );
		}
		host.Dispose();
	}

	public Program () {
		
	}
}