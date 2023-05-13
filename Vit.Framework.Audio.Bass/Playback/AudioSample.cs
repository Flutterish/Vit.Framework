using Vit.Framework.Audio.Playback;
using Vit.Framework.Memory;

namespace Vit.Framework.Audio.Bass.Playback;

public class AudioSample : DisposableObject, IAudioSample {
	public readonly int Handle;
	public readonly BassDevice Device;
	public AudioSample ( int handle, BassDevice device ) {
		Handle = handle;
		Device = device;
	}

	public void Play () {
		Device.UseDevice();
		var channel = BASS.SampleGetChannel( Handle );
		BASS.ChannelPlay( channel );
	}

	protected override void Dispose ( bool disposing ) {
		Device.UseDevice();
		BASS.SampleFree( Handle );
	}
}
