namespace Vit.Framework.DependencyInjection;

[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
public class ResolveDependencyAttribute : Attribute {
	public Type? Type;
	public string? Name;
}
