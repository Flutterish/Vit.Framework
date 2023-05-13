namespace Vit.Framework.Audio.Bass;

public class BassApi : AudioApi {
	public static readonly AudioApiType AudioApiType = new() {
		KnownName = KnownAudioApiName.Bass,
		Name = "Bass",
		Version = -1
	};

	public BassApi () : base( AudioApiType ) { // combobreak.wav
		
	}

	Dictionary<int, BassDevice> devices = new();
	BassDevice? defaultDevice;
	public override IAudioDevice? DefaultDevice {
		get {
			if ( defaultDevice != null )
				return defaultDevice;

			var count = BASS.DeviceCount;
			for ( int i = 1; i < count; i++ ) {
				var info = BASS.GetDeviceInfo( i );
				if ( info.IsDefault )
					return defaultDevice = GetAudioDevice( i );
			}

			return null;
		}
	}
	public override IEnumerable<AudioDeviceInfo> AudioDevices {
		get {
			var count = BASS.DeviceCount;
			for ( int i = 1; i < count; i++ ) {
				var info = BASS.GetDeviceInfo( i );
				if ( string.IsNullOrEmpty( info.Driver ) )
					continue;

				yield return new() {
					Index = i,
					Name = info.Name
				};
			}
		}
	}

	public override BassDevice GetAudioDevice ( int index ) {
		if ( !devices.TryGetValue( index, out var device ) ) {
			devices.Add( index, device = new( index ) );
		}

		return device;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_,i) in devices ) {
			i.Dispose();
		}
	}
}
