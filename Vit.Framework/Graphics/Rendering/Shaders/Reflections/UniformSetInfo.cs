namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class UniformSetInfo {
	public readonly List<UniformResourceInfo> Resources = new();

	public override string ToString () {
		return $"[\n\t{string.Join( "\n", Resources ).Replace( "\n", "\n\t" )}\n]";
	}
}
