using System.Diagnostics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Vulkan;

[DebuggerDisplay( "{DebuggerDisplay,nq}" )]
public abstract class VulkanObject<T> : IVulkanHandle<T> where T : unmanaged {
	protected T Instance;
	public T Handle => Instance;

	public static implicit operator T ( VulkanObject<T> obj ) => obj.Instance;

	private string DebuggerDisplay => ToString();
	public override string ToString () {
		return $"Managed {Instance.GetType().Name} [0x{MemoryMarshal.Cast<T, ulong>( MemoryMarshal.CreateSpan(ref Instance, 1) )[0]:X}]";
	}
}

[DebuggerDisplay( "{DebuggerDisplay,nq}" )]
public abstract class DisposableVulkanObject<T> : DisposableObject, IVulkanHandle<T> where T : unmanaged {
	protected T Instance;
	public T Handle => Instance;

	public static implicit operator T ( DisposableVulkanObject<T> obj ) => obj.Instance;

	private string DebuggerDisplay => ToString();
	public override string ToString () {
		return $"Managed {Instance.GetType().Name} [0x{MemoryMarshal.Cast<T, ulong>( MemoryMarshal.CreateSpan( ref Instance, 1 ) )[0]:X}]";
	}
}
