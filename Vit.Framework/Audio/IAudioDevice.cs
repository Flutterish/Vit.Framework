using Vit.Framework.Audio.Playback;

namespace Vit.Framework.Audio;

public interface IAudioDevice : IDisposable {
	AudioDeviceInfo Info { get; }

	IAudioStream LoadStream ( string path );
	IAudioSample LoadSample ( string path, int maxSimultanious );
}

public struct AudioDeviceInfo {
	public int Index;
	public string Name;
}