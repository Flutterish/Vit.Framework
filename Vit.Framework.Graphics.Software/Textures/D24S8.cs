using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace Vit.Framework.Graphics.Software.Textures;

/// <summary>
/// Represents a depth (24b) stencil (8b) value.
/// </summary>
public struct D24S8 : IPixel<D24S8> {
	public uint Packed;
	public uint DepthData {
		get => Packed >> 8;
		set => Packed = ( value << 8 ) | ( Packed & 0xFF );
	}
	public float Depth {
		get => MathF.Pow( 0.5f, 23 ) * DepthData;
		set => DepthData = (uint)( MathF.Pow( 2, 23 ) * value );
	}
	public byte Stencil {
		get => (byte)( Packed & 0xFF );
		set => Packed = ( ( Packed >> 8 ) << 8 ) | value;
	}

	public PixelOperations<D24S8> CreatePixelOperations () {
		throw new NotImplementedException();
	}

	public void FromScaledVector4 ( Vector4 vector ) {
		throw new NotImplementedException();
	}

	public Vector4 ToScaledVector4 () {
		throw new NotImplementedException();
	}

	public void FromVector4 ( Vector4 vector ) {
		throw new NotImplementedException();
	}

	public Vector4 ToVector4 () {
		throw new NotImplementedException();
	}

	public void FromArgb32 ( Argb32 source ) {
		throw new NotImplementedException();
	}

	public void FromBgra5551 ( Bgra5551 source ) {
		throw new NotImplementedException();
	}

	public void FromBgr24 ( Bgr24 source ) {
		throw new NotImplementedException();
	}

	public void FromBgra32 ( Bgra32 source ) {
		throw new NotImplementedException();
	}

	public void FromAbgr32 ( Abgr32 source ) {
		throw new NotImplementedException();
	}

	public void FromL8 ( L8 source ) {
		throw new NotImplementedException();
	}

	public void FromL16 ( L16 source ) {
		throw new NotImplementedException();
	}

	public void FromLa16 ( La16 source ) {
		throw new NotImplementedException();
	}

	public void FromLa32 ( La32 source ) {
		throw new NotImplementedException();
	}

	public void FromRgb24 ( Rgb24 source ) {
		throw new NotImplementedException();
	}

	public void FromRgba32 ( Rgba32 source ) {
		throw new NotImplementedException();
	}

	public void ToRgba32 ( ref Rgba32 dest ) {
		throw new NotImplementedException();
	}

	public void FromRgb48 ( Rgb48 source ) {
		throw new NotImplementedException();
	}

	public void FromRgba64 ( Rgba64 source ) {
		throw new NotImplementedException();
	}

	public bool Equals ( D24S8 other ) {
		return Packed == other.Packed;
	}

	public override string ToString () {
		return $"S: 0x{Stencil:X2} D: {Depth}";
	}
}