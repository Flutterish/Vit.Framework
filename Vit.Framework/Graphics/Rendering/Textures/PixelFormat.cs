using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Textures;

/// <summary>
/// Specifies how texels are stored. It does not, however, specify how they are interpreted by a shader or displayed.
/// <list type="bullet">
///		<item>The first part specifies how many components are stored.</item>
///		<item>The following list specifies the sizes of each component.</item>
///		<item>Any following types specify the type for the previous components.</item>
/// </list>
/// </summary>
public enum TexelFormat {
	Vec4_8_8_8_8_Unorm,
	Vec3_8_8_8_Unorm,
	Vec2_8_8_Unorm,
	Vec1_8_Unorm,

	Vec1_24_Unorm_8_Pad,
	Vec1_32_Fp,
	Vec2_24_Unorm_8_UInt,
	Vec2_32_Fp_8_UInt_24_Pad
}

public enum ChannelName {
	Undefined,
	Red,
	Green,
	Blue,
	Alpha,
	Depth,
	Stencil
}

/// <summary>
/// Specifies how a pixel is stored and its channels interpreted.
/// </summary>
public struct PixelFormat {
	public TexelFormat Format;

	public ChannelName Channel1;
	public ChannelName Channel2;
	public ChannelName Channel3;
	public ChannelName Channel4;

	public static bool operator == ( PixelFormat left, PixelFormat right ) {
		return left.Format == right.Format && MemoryMarshal.CreateSpan( ref left.Channel1, 4 ).SequenceEqual( MemoryMarshal.CreateSpan( ref right.Channel1, 4 ) );
	}

	public static bool operator != ( PixelFormat left, PixelFormat right )
		=> !(left == right);

	public static readonly PixelFormat Rgba8 = new() {
		Format = TexelFormat.Vec4_8_8_8_8_Unorm,
		Channel1 = ChannelName.Red,
		Channel2 = ChannelName.Green,
		Channel3 = ChannelName.Blue,
		Channel4 = ChannelName.Alpha
	};
	public static readonly PixelFormat Rgb8 = new() {
		Format = TexelFormat.Vec3_8_8_8_Unorm,
		Channel1 = ChannelName.Red,
		Channel2 = ChannelName.Green,
		Channel3 = ChannelName.Blue
	};
	public static readonly PixelFormat D24 = new() {
		Format = TexelFormat.Vec1_24_Unorm_8_Pad,
		Channel1 = ChannelName.Depth
	};
	public static readonly PixelFormat D24S8ui = new() {
		Format = TexelFormat.Vec2_24_Unorm_8_UInt,
		Channel1 = ChannelName.Depth,
		Channel2 = ChannelName.Stencil
	};
	public static readonly PixelFormat D32f = new() {
		Format = TexelFormat.Vec1_32_Fp,
		Channel1 = ChannelName.Depth
	};
	public static readonly PixelFormat D32fS8ui = new() {
		Format = TexelFormat.Vec2_32_Fp_8_UInt_24_Pad,
		Channel1 = ChannelName.Depth,
		Channel2 = ChannelName.Stencil
	};
}
