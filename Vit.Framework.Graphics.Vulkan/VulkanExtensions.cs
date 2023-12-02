using System.Runtime.CompilerServices;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public static class VulkanExtensions {
	public static unsafe class Out<T> where T : unmanaged {
		public delegate VkResult PtrParamResultEnumerator<P> ( P* param, ref uint count, T* array ) where P : unmanaged;
		public static unsafe T[] Enumerate<P> ( P* param, PtrParamResultEnumerator<P> enumerator ) where P : unmanaged {
			uint count = 0;
			Validate( enumerator( param, ref count, (T*)0 ) );
			T[] array = new T[count];
			fixed ( T* arrayPtr = array ) {
				Validate( enumerator( param, ref count, arrayPtr ) );
			}
			return array;
		}

		public delegate VkResult ParamResultEnumerator<P> ( P param, ref uint count, T* array );
		public static T[] Enumerate<P> ( P param, ParamResultEnumerator<P> enumerator ) {
			uint count = 0;
			Validate( enumerator( param, ref count, (T*)0 ) );
			T[] array = new T[count];
			fixed ( T* arrayPtr = array ) {
				Validate( enumerator( param, ref count, arrayPtr ) );
			}
			return array;
		}

		public delegate void VoidParamResultEnumerator<P> ( P param, ref uint count, T* array );
		public static T[] Enumerate<P> ( P param, VoidParamResultEnumerator<P> enumerator ) {
			uint count = 0;
			enumerator( param, ref count, (T*)0 );
			T[] array = new T[count];
			fixed ( T* arrayPtr = array ) {
				enumerator( param, ref count, arrayPtr );
			}
			return array;
		}

		public delegate VkResult ResultEnumerator ( ref uint count, T* array );
		public static unsafe T[] Enumerate ( ResultEnumerator enumerator ) {
			uint count = 0;
			Validate( enumerator( ref count, (T*)0 ) );
			T[] array = new T[count];
			fixed ( T* arrayPtr = array ) {
				Validate( enumerator( ref count, arrayPtr ) );
			}
			return array;
		}

		public delegate VkResult ParamPtrParamResultEnumerator<P, P2> ( P param, P2* param2, ref uint count, T* array ) where P2 : unmanaged;
		public static unsafe T[] Enumerate<P, P2> ( P param, P2* param2, ParamPtrParamResultEnumerator<P, P2> enumerator ) where P2 : unmanaged {
			uint count = 0;
			Validate( enumerator( param, param2, ref count, (T*)0 ) );
			T[] array = new T[count];
			fixed ( T* arrayPtr = array ) {
				Validate( enumerator( param, param2, ref count, arrayPtr ) );
			}
			return array;
		}

		public delegate VkResult ParamParamResultEnumerator<P, P2> ( P param, P2 param2, ref uint count, T* array );
		public static unsafe T[] Enumerate<P, P2> ( P param, P2 param2, ParamParamResultEnumerator<P, P2> enumerator ) {
			uint count = 0;
			Validate( enumerator( param, param2, ref count, (T*)0 ) );
			T[] array = new T[count];
			fixed ( T* arrayPtr = array ) {
				Validate( enumerator( param, param2, ref count, arrayPtr ) );
			}
			return array;
		}
	}

	public static void Validate ( this VkResult result, [CallerArgumentExpression(nameof(result))] string? expression = null ) {
		if ( result != VkResult.Success )
			throw new Exception( $"Operation failed: {result} at {expression}" );
	}

	public static unsafe string GetName ( this VkExtensionProperties properties ) {
		return InteropExtensions.GetString( properties.extensionName, 256 );
	}

	public static unsafe string GetName ( this VkLayerProperties properties ) {
		return InteropExtensions.GetString( properties.layerName, 256 );
	}

	public static unsafe string GetDescription ( this VkLayerProperties properties ) {
		return InteropExtensions.GetString( properties.description, 256 );
	}

	public static VkCompareOp CompareOp ( this CompareOperation compare ) => compare switch {
		CompareOperation.Always => VkCompareOp.Always,
		CompareOperation.LessThan => VkCompareOp.Less,
		CompareOperation.GreaterThan => VkCompareOp.Greater,
		CompareOperation.Equal => VkCompareOp.Equal,
		CompareOperation.NotEqual => VkCompareOp.NotEqual,
		CompareOperation.LessThanOrEqual => VkCompareOp.Less,
		CompareOperation.GreaterThanOrEqual => VkCompareOp.GreaterOrEqual,
		CompareOperation.Never or _ => VkCompareOp.Never
	};

	public static VkStencilOp StencilOp ( this StencilOperation operation ) => operation switch {
		StencilOperation.SetTo0 => VkStencilOp.Zero,
		StencilOperation.ReplaceWithReference => VkStencilOp.Replace,
		StencilOperation.Invert => VkStencilOp.Invert,
		StencilOperation.Increment => VkStencilOp.IncrementAndClamp,
		StencilOperation.Decrement => VkStencilOp.DecrementAndClamp,
		StencilOperation.IncrementWithWrap => VkStencilOp.IncrementAndWrap,
		StencilOperation.DecrementWithWrap => VkStencilOp.DecrementAndWrap,
		StencilOperation.Keep or _ => VkStencilOp.Keep
	};

	public static unsafe VkAllocationCallbacks* TODO_Allocator => (VkAllocationCallbacks*)0;
}
