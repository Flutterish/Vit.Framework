namespace Vit.Framework.Audio.Playback;

public interface IAudioSample : IPlayableAudio, IDisposable {
	void TransferTo ( IAudioDevice device );
}
