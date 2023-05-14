namespace Vit.Framework.DependencyInjection;

[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class )]
public class CacheDependencyAttribute : Attribute {
	public Type? Type;
	public string? Name;
}
