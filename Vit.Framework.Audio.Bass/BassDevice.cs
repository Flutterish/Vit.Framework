using ManagedBass;
using Vit.Framework.Audio.Bass.Playback;
using Vit.Framework.Audio.Playback;
using Vit.Framework.Memory;

namespace Vit.Framework.Audio.Bass;

public class BassDevice : DisposableObject, IAudioDevice {
	public AudioDeviceInfo Info { get; }
	public readonly BassInfo BassInfo;
	public BassDevice ( int index ) {
		BASS.Init( index );
		index = BASS.CurrentDevice;
		var info = BASS.GetDeviceInfo( index );
		Info = new() {
			Index = index,
			Name = info.Name
		};
		UseDevice();

		BASS.GetInfo( out BassInfo );
	}

	[ThreadStatic]
	static int boundDevice; 
	public void UseDevice () {
		if ( boundDevice == Info.Index )
			return;

		BASS.CurrentDevice = boundDevice = Info.Index;
	}

	public IAudioStream LoadStream ( string path ) {
		UseDevice();
		var handle = BASS.CreateStream( path );

		return new AudioStream( handle, this );
	}

	public IAudioSample LoadSample ( string path, int maxSimultanious ) {
		UseDevice();
		var handle = BASS.SampleLoad( path, 0, 0, maxSimultanious, BassFlags.Default );

		return new AudioSample( handle, this );
	}

	protected override void Dispose ( bool disposing ) {
		UseDevice();
		BASS.Free();
	}
}
