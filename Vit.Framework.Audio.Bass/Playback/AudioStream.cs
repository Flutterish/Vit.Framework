using Vit.Framework.Audio.Playback;
using Vit.Framework.Memory;

namespace Vit.Framework.Audio.Bass.Playback;

public class AudioStream : DisposableObject, IAudioStream {
	public readonly int Handle;
	public readonly BassDevice Device;
	public AudioStream ( int handle, BassDevice device ) {
		Handle = handle;
		Device = device;
	}

	public void Play () {
		Device.UseDevice();
		BASS.ChannelPlay( Handle );
	}

	protected override void Dispose ( bool disposing ) {
		Device.UseDevice();
		BASS.StreamFree( Handle );
	}
}
