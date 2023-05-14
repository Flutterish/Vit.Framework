using Vit.Framework.Audio.Playback;
using Vit.Framework.Memory;

namespace Vit.Framework.Audio.Bass.Playback;

public class AudioStream : DisposableObject, IAudioStream {
	public readonly int Handle;
	public BassDevice Device { get; private set; }
	public AudioStream ( int handle, BassDevice device ) {
		Handle = handle;
		Device = device;
	}

	public void Play () {
		//Device.UseDevice();
		BASS.ChannelPlay( Handle );
	}

	public void TransferTo ( IAudioDevice device ) {
		Device = (BassDevice)device;
		BASS.ChannelSetDevice( Handle, Device.Info.Index );
	}

	protected override void Dispose ( bool disposing ) {
		Device.UseDevice();
		BASS.StreamFree( Handle );
	}
}
