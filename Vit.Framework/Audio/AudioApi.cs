using Vit.Framework.Memory;

namespace Vit.Framework.Audio;

public abstract class AudioApi : DisposableObject {
	public readonly AudioApiType API;

	public AudioApi ( AudioApiType aPI ) {
		API = aPI;
	}

	public abstract IAudioDevice? DefaultDevice { get; }
	public abstract IEnumerable<AudioDeviceInfo> AudioDevices { get; }
	public abstract IAudioDevice GetAudioDevice ( int index );
}
