namespace Vit.Framework.Audio;

public record AudioApiType {
	public KnownAudioApiName? KnownName { get; init; }
	public required string Name { get; init; }
	public required int Version { get; init; }

	public override string ToString () {
		return Version <= 0 ? Name : $"{Name} [Version {Version}]";
	}
}

public enum KnownAudioApiName {
	Bass
}
