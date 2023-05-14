using Vit.Framework.Audio.Playback;
using Vit.Framework.Memory;

namespace Vit.Framework.Audio.Bass.Playback;

public class AudioSample : DisposableObject, IAudioSample {
	public readonly int Handle;
	public BassDevice Device { get; private set; }
	public AudioSample ( int handle, BassDevice device ) {
		Handle = handle;
		Device = device;
	}

	public void Play () {
		//Device.UseDevice();
		var channel = BASS.SampleGetChannel( Handle );
		BASS.ChannelPlay( channel );
	}

	public void TransferTo ( IAudioDevice device ) {
		Device = (BassDevice)device;
		BASS.ChannelSetDevice( Handle, Device.Info.Index );
	}

	protected override void Dispose ( bool disposing ) {
		Device.UseDevice();
		BASS.SampleFree( Handle );
	}
}
