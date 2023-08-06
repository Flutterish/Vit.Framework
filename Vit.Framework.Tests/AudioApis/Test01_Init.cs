using Vit.Framework.Audio;
using Vit.Framework.Audio.Bass;
using Vit.Framework.Audio.Playback;
using Vit.Framework.Threading;

namespace Vit.Framework.Tests.AudioApis;

public class Test01_Samples : AppThread {
	AudioApi api = null!;
	public Test01_Samples ( string name ) : base( name ) {

	}

	IPlayableAudio audio = null!;
	protected override bool Initialize () {
		api = new BassApi();
		audio = api.DefaultDevice!.LoadSample( "./combobreak.wav", maxSimultanious: 8 );

		return true;
	}

	protected override void Loop () {
		audio.Play();
		Sleep( 50 );
	}

	protected override void Dispose ( bool disposing ) {
		api.Dispose();
	}
}
