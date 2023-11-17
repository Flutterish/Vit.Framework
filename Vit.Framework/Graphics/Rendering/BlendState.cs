using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering;

public struct BlendState {
	public required bool IsEnabled;

	public BlendFactor FragmentFactor {
		set => FragmentColorFactor = FragmentAlphaFactor = value;
	}
	public BlendFactor DestinationFactor {
		set => DestinationColorFactor = DestinationAlphaFactor = value;
	}
	public BlendFunction Function {
		set => ColorFunction = AlphaFunction = value;
	}

	public BlendFactor FragmentColorFactor;
	public BlendFactor DestinationColorFactor;
	public BlendFactor FragmentAlphaFactor;
	public BlendFactor DestinationAlphaFactor;
	public BlendFunction ColorFunction;
	public BlendFunction AlphaFunction;

	public Vector4<float> Constant;

	public static readonly BlendState PremultipliedAlpha = new() {
		IsEnabled = true,
		FragmentFactor = BlendFactor.One,
		DestinationFactor = BlendFactor.FragmentAlphaInverse,
		Function = BlendFunction.Add
	};

	public override string ToString () {
		var c = Constant;
		string colorPart ( string name, BlendFactor factor ) {
			var part = factor switch {
				BlendFactor.Zero => "0, 0, 0",
				BlendFactor.One => "1, 1, 1",
				BlendFactor.Fragment => "frag.r, frag.g, frag.b",
				BlendFactor.FragmentInverse => "1 - frag.r, 1 - frag.g, 1 - frag.b",
				BlendFactor.Destination => "dst.r, dst.g, dst.b",
				BlendFactor.DestinationInverse => "1 - dst.r, 1 - dst.g, 1 - dst.b",
				BlendFactor.FragmentAlpha => "frag.a, frag.a, frag.a",
				BlendFactor.FragmentAlphaInverse => "1 - frag.a, 1 - frag.a, 1 - frag.a",
				BlendFactor.DestinationAlpha => "dst.a, dst.a, dst.a",
				BlendFactor.DestinationAlphaInverse => "1 - dst.a, 1 - dst.a, 1 - dst.a",
				BlendFactor.Constant => $"{c.X}, {c.Y}, {c.Z}",
				BlendFactor.ConstantInverse => $"{1 - c.X}, {1 - c.Y}, {1 - c.Z}",
				BlendFactor.ConstantAlpha => $"{c.W}, {c.W}, {c.W}",
				BlendFactor.ConstantAlphaInverse => $"{1 - c.W}, {1 - c.W}, {1 - c.W}",
				BlendFactor.AlphaSaturate => "min(frag.a, 1 - dst.a), min(frag.a, 1 - dst.a), min(frag.a, 1 - dst.a)",
				BlendFactor.SecondFragment => "frag2.r, frag2.g, frag2.b",
				BlendFactor.SecondFragmentInverse => "1 - frag2.r, 1 - frag2.g, 1 - frag2.b",
				BlendFactor.SecondFragmentAlpha => "frag2.a, frag2.a, frag2.a",
				BlendFactor.SecondFragmentAlphaInverse => "1 - frag2.a, 1 - frag2.a, 1 - frag2.a",
				_ => "???"
			};

			return (factor, c) is (BlendFactor.Zero, _) or (BlendFactor.Constant, (0, 0, 0, _)) 
				or (BlendFactor.ConstantInverse, (1, 1, 1, _)) or (BlendFactor.ConstantAlpha, (_, _, _, 0))
				or (BlendFactor.ConstantAlphaInverse, (_, _, _, 1))
				? part
				: (factor, c) is (BlendFactor.One, _) or (BlendFactor.Constant, (1, 1, 1, _)) 
				or (BlendFactor.ConstantInverse, (0, 0, 0, _)) or (BlendFactor.ConstantAlpha, (_, _, _, 1))
				or (BlendFactor.ConstantAlphaInverse, (_, _, _, 0))
				? name
				: $"{name} * ({part})";
		}

		string alphaPart ( string name, BlendFactor factor ) {
			var part = factor switch {
				BlendFactor.Zero => "0",
				BlendFactor.One => "1",
				BlendFactor.Fragment => "frag.a",
				BlendFactor.FragmentInverse => "1 - frag.a",
				BlendFactor.Destination => "dst.a",
				BlendFactor.DestinationInverse => "1 - dst.a",
				BlendFactor.FragmentAlpha => "frag.a",
				BlendFactor.FragmentAlphaInverse => "1 - frag.a",
				BlendFactor.DestinationAlpha => "dst.a",
				BlendFactor.DestinationAlphaInverse => "1 - dst.a",
				BlendFactor.Constant => $"{c.W}",
				BlendFactor.ConstantInverse => $"{1 - c.W}",
				BlendFactor.ConstantAlpha => $"{c.W}",
				BlendFactor.ConstantAlphaInverse => $"{1 - c.W}",
				BlendFactor.AlphaSaturate => "1",
				BlendFactor.SecondFragment => "frag2.a",
				BlendFactor.SecondFragmentInverse => "1 - frag2.a",
				BlendFactor.SecondFragmentAlpha => "frag2.a",
				BlendFactor.SecondFragmentAlphaInverse => "1 - frag2.a",
				_ => "???"
			};

			return (factor, c) is (BlendFactor.Zero, _) or (BlendFactor.Constant or BlendFactor.ConstantAlpha, (_, _, _, 0))
				or (BlendFactor.ConstantInverse or BlendFactor.ConstantAlphaInverse, (_, _, _, 1))
				? part
				: (factor, c) is (BlendFactor.One, _) or (BlendFactor.Constant or BlendFactor.ConstantAlpha, (_, _, _, 1))
				or (BlendFactor.ConstantInverse or BlendFactor.ConstantAlphaInverse, (_, _, _, 0))
				? name
				: $"{name} * ({part})";
		}

		if ( !IsEnabled )
			return "Disabled";

		var partA = ColorFunction switch {
			BlendFunction.Add => $"{colorPart( "frag.rgb", FragmentColorFactor )} + {colorPart( "dst.rgb", DestinationColorFactor )}",
			BlendFunction.Max => $"max(frag.rgb, dst.rgb)",
			BlendFunction.Min => $"min(frag.rgb, dst.rgb)",
			BlendFunction.FragmentMinusDestination => $"{colorPart( "frag.rgb", FragmentColorFactor )} - {colorPart( "dst.rgb", DestinationColorFactor )}",
			BlendFunction.DestinationMinusFragment => $"{colorPart( "dst.rgb", DestinationColorFactor )} - {colorPart( "frag.rgb", FragmentColorFactor )}",
			_ => "???"
		};

		var partB = AlphaFunction switch {
			BlendFunction.Add => $"{alphaPart( "frag.a", FragmentAlphaFactor )} + {alphaPart( "dst.a", DestinationAlphaFactor )}",
			BlendFunction.Max => $"max(frag.a, dst.a)",
			BlendFunction.Min => $"min(frag.a, dst.a)",
			BlendFunction.FragmentMinusDestination => $"{alphaPart( "frag.a", FragmentAlphaFactor )} - {alphaPart( "dst.a", DestinationAlphaFactor )}",
			BlendFunction.DestinationMinusFragment => $"{alphaPart( "dst.a", DestinationAlphaFactor )} - {alphaPart( "frag.a", FragmentAlphaFactor )}",
			_ => "???"
		};

		return $"RGB = {partA}; A = {partB}";
	}
}

/// <summary>
/// Specifies a factor by which a blending equation side is multiplied.
/// </summary>
public enum BlendFactor {
	/// <summary>
	/// The value is multiplied by 0.
	/// </summary>
	Zero,
	/// <summary>
	/// The value is not modified.
	/// </summary>
	One,
	/// <summary>
	/// The value is memberwise multiplied by the incoming value from the fragment shader.
	/// </summary>
	Fragment,
	/// <summary>
	/// The value is memberwise multiplied by one minus the value incoming from the fragment shader.
	/// </summary>
	FragmentInverse,
	/// <summary>
	/// The value is memberwise multiplied by the value from the frame buffer.
	/// </summary>
	Destination,
	/// <summary>
	/// The value is memberwise multiplied by one minus the value from the frame buffer.
	/// </summary>
	DestinationInverse,
	/// <summary>
	/// The value is multiplied by the alpha incoming from the fragment shader.
	/// </summary>
	FragmentAlpha,
	/// <summary>
	/// The value is multiplied by one minus the alpha incoming from the fragment shader.
	/// </summary>
	FragmentAlphaInverse,
	/// <summary>
	/// The value is multiplied by the alpha from the frame buffer.
	/// </summary>
	DestinationAlpha,
	/// <summary>
	/// The value is multiplied by one minus the alpha from the frame buffer.
	/// </summary>
	DestinationAlphaInverse,

	/// <summary>
	/// The value is memberwise multiplied by the specified constant.
	/// </summary>
	Constant,
	/// <summary>
	/// The value is memberwise multiplied by one minus the specified constant.
	/// </summary>
	ConstantInverse,
	/// <summary>
	/// The value is memberwise multiplied by the specified alpha constant.
	/// </summary>
	ConstantAlpha,
	/// <summary>
	/// The value is memberwise multiplied by one minus the specified alpha constant.
	/// </summary>
	ConstantAlphaInverse,

	/// <summary>
	/// The RGB values are multiplied by <c>min(incoming alpha, 1 - framebuffer alpha)</c>. Alpha is not modified.
	/// </summary>
	AlphaSaturate,
	/// <summary>
	/// The value is memberwise multiplied by the second value incoming from the fragment shader.
	/// </summary>
	SecondFragment,
	/// <summary>
	/// The value is memberwise multiplied by one minus the second value incoming from the fragment shader.
	/// </summary>
	SecondFragmentInverse,
	/// <summary>
	/// The value is multiplied by the second alpha incoming from the fragment shader.
	/// </summary>
	SecondFragmentAlpha,
	/// <summary>
	/// The value is multiplied by one minus the second alpha incoming from the fragment shader.
	/// </summary>
	SecondFragmentAlphaInverse
} 

public enum BlendFunction {
	Add,
	/// <summary>
	/// Componentwise maximum. Ignores blending factors.
	/// </summary>
	Max,
	/// <summary>
	/// Componentwise minimum. Ignores blending factors.
	/// </summary>
	Min,
	FragmentMinusDestination,
	DestinationMinusFragment
}