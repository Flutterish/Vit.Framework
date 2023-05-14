namespace Vit.Framework.Audio.Playback;

public interface IAudioStream : IPlayableAudio, IDisposable {
	void TransferTo ( IAudioDevice device );
}
