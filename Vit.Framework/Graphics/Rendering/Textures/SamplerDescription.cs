namespace Vit.Framework.Graphics.Rendering.Textures;

public struct SamplerDescription {
	public FilteringMode MagnificationFilter;
	public FilteringMode MinificationFilter;
	public MipmapMode MipmapMode;

	public float MinimimMipmapLevel;
	public float MaximimMipmapLevel;
	public float MipmapLevelBias;

	public TextureWrapMode WrapU;
	public TextureWrapMode WrapV;

	public bool EnableAnisotropy;
	public float MaximumAnisotropicFiltering;

	public static readonly SamplerDescription DefaultPixelated = new() {
		MagnificationFilter = FilteringMode.Nearest,
		MinificationFilter = FilteringMode.Nearest,
		MipmapMode = MipmapMode.None,

		WrapV = TextureWrapMode.ClampToEdge,
		WrapU = TextureWrapMode.ClampToEdge
	};

	public static readonly SamplerDescription DefaultBilinear = DefaultPixelated with {
		MagnificationFilter = FilteringMode.Linear,
		MinificationFilter = FilteringMode.Linear
	};

	public readonly bool UsesBorderColour 
		=> WrapV is TextureWrapMode.TransparentBlackBorder 
		|| WrapU is TextureWrapMode.TransparentBlackBorder;
}

/// <summary>
/// Specifies how colour values are interpolated when sampling an image.
/// </summary>
public enum FilteringMode {
	/// <summary>
	/// Returns the closest pixel's colour.
	/// </summary>
	Nearest,
	/// <summary>
	/// Returns a weighted average of 4 closest pixels' colours.
	/// </summary>
	Linear
}

/// <summary>
/// Specifies how mipmaps are used.
/// </summary>
public enum MipmapMode {
	/// <summary>
	/// Mipmaps are not used.
	/// </summary>
	None,
	/// <summary>
	/// Chooses the closest mipmap.
	/// </summary>
	Nearest,
	/// <summary>
	/// Chooses 2 closest mipmaps and returns a weigthed average of the two.
	/// </summary>
	Linear
}

/// <summary>
/// Specifies what happens when UV coordinates are outside the [0;1] range.
/// </summary>
public enum TextureWrapMode {
	/// <summary>
	/// The coordinates will wrap back around, as if using modulo. This will cause a texture to repeat itself.
	/// </summary>
	Repeat,
	/// <summary>
	/// The coordinates will bounce back between 0 and 1. This will cause a texture to repeat and be mirrored every tile.
	/// </summary>
	MirroredRepeat,
	/// <summary>
	/// The coordinates will be clamped between 0 and 1. This will cause a texture to "smear" out the edge.
	/// </summary>
	ClampToEdge,
	/// <summary>
	/// When the coordinates are outside the range transparent black will be returned instead.
	/// </summary>
	TransparentBlackBorder // TODO a custom color extension, vulkan for some reason does not have custom border colours by default
}