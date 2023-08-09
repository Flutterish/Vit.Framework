namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public enum PrimitiveType {
	Void,
	Struct,

	Float32,
	Int32,
	UInt32,

	Sampler
}

public static class PrimitiveTypeExtensions {
	public static uint SizeOf ( this PrimitiveType type ) {
		return type switch {
			PrimitiveType.Void => throw new InvalidOperationException(),
			PrimitiveType.Struct => throw new InvalidOperationException(),
			PrimitiveType.Float32 => 4,
			PrimitiveType.Int32 => 4,
			PrimitiveType.UInt32 => 4,
			PrimitiveType.Sampler => throw new InvalidOperationException(),

			_ => throw new NotImplementedException(),
		};
	}
}