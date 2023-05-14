namespace Vit.Framework.DependencyInjection;

public struct DependencyIdentifier {
	public required Type Type;
	public string? Name;

	public override string ToString () {
		return Name is null ? $"{Type}" : $"{Type} [{Name}]";
	}
}
