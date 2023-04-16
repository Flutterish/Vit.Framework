using System.Reflection;
using System.Runtime.InteropServices;

namespace Vit.Framework.Parsing;

public class EndianCorrectingBinaryReader : IDisposable {
	public bool IsStreamLittleEndian;
	public static readonly bool IsSystemLittleEndian = BitConverter.IsLittleEndian;
	public readonly Stream Stream;
	public EndianCorrectingBinaryReader ( Stream input, bool isLitteEndian ){
		IsStreamLittleEndian = isLitteEndian;
		Stream = input;
	}

	byte[] buffer = new byte[8];
	public int Capacity => buffer.Length;
	public void EnsureCapacity ( int capacity ) {
		if ( Capacity < capacity )
			buffer = new byte[capacity];
	}

	public ReadOnlySpan<byte> Read ( int count ) {
		var data = buffer.AsSpan( 0, count );
		Stream.ReadExactly( data );
		if ( IsStreamLittleEndian != IsSystemLittleEndian )
			data.Reverse();
		return data;
	}

	public ReadOnlySpan<byte> Read ( int count, int padding ) {
		Span<byte> data;
		if ( IsSystemLittleEndian ) { // pad in front
			data = buffer.AsSpan( padding, count );
			buffer.AsSpan( 0, padding ).Fill( 0 );
		}
		else { // pad at end
			data = buffer.AsSpan( 0, count );
			buffer.AsSpan( count, padding ).Fill( 0 );
		}

		Stream.ReadExactly( data );
		if ( IsStreamLittleEndian != IsSystemLittleEndian )
			data.Reverse();

		return buffer.AsSpan( 0, count + padding );
	}

	public T Read<T> () where T : unmanaged, IConvertible {
		if (typeof(T).IsEnum) {
			var underlying = typeof( T ).GetEnumUnderlyingType();

			var value = typeof( EndianCorrectingBinaryReader ).GetMethod( nameof( Read ), Array.Empty<Type>() )!.MakeGenericMethod( underlying ).Invoke( this, Array.Empty<object?>() );
			return (T)value!;
		}
		else {
			var size = Marshal.SizeOf<T>();
			var data = Read( size );

			return MemoryMarshal.Read<T>( data );
		}
	}

	[ThreadStatic]
	static Dictionary<Type, MethodInfo>? readMethods;
	public object? Read ( Type type ) {
		if ( !(readMethods ??= new()).TryGetValue( type, out var method ) )
			readMethods.Add( type, method = typeof(EndianCorrectingBinaryReader).GetMethod(nameof(Read), 1, Array.Empty<Type>())!.MakeGenericMethod( type ) );

		return method.Invoke( this, Array.Empty<object?>() );
	}

	public void Dispose () {
		Stream.Dispose();
	}
}
