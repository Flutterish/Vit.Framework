namespace Vit.Framework.Graphics.Rendering;

public record GraphicsApiType {
	public KnownGraphicsApiName? KnownName { get; init; }
	public required string Name { get; init; }
	public required int Version { get; init; }

	public override string ToString () {
		return Version <= 0 ? Name : $"{Name} [Version {Version}]";
	}
}

public enum KnownGraphicsApiName {
	OpenGl,
	OpenGlEs,
	Vulkan,
	Direct3D11
}
