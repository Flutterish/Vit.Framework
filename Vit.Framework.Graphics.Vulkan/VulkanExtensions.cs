using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vit.Framework.Allocation;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public static class VulkanExtensions {
	public static class Out<TOut> {
		public delegate K ParamUCounter<K, P> ( P param, out uint count, TOut[]? values );
		public delegate K HandleParamUCounter<K, P> ( P param, out uint count, nint values );
		public delegate void HandleParamUCounter<P> ( P param, out uint count, nint values );
		public delegate K Handle2ParamUCounter<K, P, P2> ( P param1, P2 param2, out uint count, nint values );
		public delegate K HandleUCounter<K> ( out uint count, nint values );

		public static TOut[] Enumerate<K, P> ( P param, ParamUCounter<K, P> counter ) {
			counter( param, out var count, null );
			TOut[] values = new TOut[count];
			counter( param, out count, values );
			return values;
		}
		public static TOut[] Enumerate<K, P> ( P param, HandleParamUCounter<K, P> counter ) {
			counter( param, out var count, 0 );
			TOut[] values = new TOut[count];
			using ( var hanle = new PinnedHandle( values ) )
				counter( param, out count, hanle );
			return values;
		}
		public static TOut[] Enumerate<K, P, P2> ( P param1, P2 param2, Handle2ParamUCounter<K, P, P2> counter ) {
			counter( param1, param2, out var count, 0 );
			TOut[] values = new TOut[count];
			using ( var hanle = new PinnedHandle( values ) )
				counter( param1, param2, out count, hanle );
			return values;
		}
		public static TOut[] Enumerate<P> ( P param, HandleParamUCounter<P> counter ) {
			counter( param, out var count, 0 );
			TOut[] values = new TOut[count];
			using ( var hanle = new PinnedHandle( values ) )
				counter( param, out count, hanle );
			return values;
		}
		public static TOut[] Enumerate<K> ( HandleUCounter<K> counter ) {
			counter( out var count, 0 );
			TOut[] values = new TOut[count];
			using ( var hanle = new PinnedHandle( values ) )
				counter( out count, hanle );
			return values;
		}
	}

	public static string GetString ( this nint ptr ) {
		return Marshal.PtrToStringUTF8( ptr ) ?? string.Empty;
	}
	public static string GetString ( this ExtensionName_char_proxy value ) {
		var str = value.ToString();
		return str.Substring( 0, str.IndexOf( '\0' ) );
	}
	public static string GetString ( this Description_char_proxy value ) {
		var str = value.ToString();
		return str.Substring( 0, str.IndexOf( '\0' ) );
	}
	public static string GetString ( this PhysicalDeviceName_char_proxy value ) {
		var str = value.ToString();
		return str.Substring( 0, str.IndexOf( '\0' ) );
	}

	public static DisposeAction<GCHandle[]> CreatePointerArray<T> ( IReadOnlyList<T> list, out nint pointer ) {
		GCHandle[] handles = new GCHandle[ list.Count + 1 ];
		nint[] pointers = new nint[list.Count];
		for ( int i = 0; i < list.Count; i++ ) {
			if ( list[i] is string str ) {
				var data = Encoding.UTF8.GetBytes( str + '\0' );
				handles[i] = GCHandle.Alloc( data, GCHandleType.Pinned );
				pointers[i] = handles[i].AddrOfPinnedObject();
			}
			else {
				handles[i] = GCHandle.Alloc( list[i], GCHandleType.Pinned );
				pointers[i] = handles[i].AddrOfPinnedObject();
			}
		}
		handles[^1] = GCHandle.Alloc( pointers, GCHandleType.Pinned );
		pointer = handles[^1].AddrOfPinnedObject();

		return new( handles, handles => {
			foreach ( var i in handles ) {
				i.Free();
			}
		} );
	}

	public static void Validate ( VkResult result, [CallerArgumentExpression(nameof(result))] string? expression = null ) {
		if ( result != VkResult.Success )
			throw new Exception( $"Operation failed: {result} at {expression}" );
	}
}
