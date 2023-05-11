using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public class RuntimeImageType : IRuntimeType {
	public IRuntimeType PixelType;
	public RuntimeImageType ( IRuntimeType pixelType ) {
		PixelType = pixelType;
	}

	public int Size { get; }

	public IRuntimeType Vectorize ( uint count ) {
		throw new NotImplementedException();
	}

	public IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public object Parse ( ReadOnlySpan<byte> data ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"Image<{PixelType}>";
	}
}

public class RuntimeSamplerType : IRuntimeType {
	public IRuntimeType ImageType;
	public RuntimeSamplerType ( IRuntimeType imageType ) {
		ImageType = imageType;
	}

	public int Size { get; } = Marshal.SizeOf( default( OpaqueHandle ) );

	public IRuntimeType Vectorize ( uint count ) {
		throw new NotImplementedException();
	}

	public IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public object Parse ( ReadOnlySpan<byte> data ) {
		return MemoryMarshal.Read<OpaqueHandle>( data );
	}

	public override string ToString () {
		return $"Sampler of {ImageType}";
	}
}