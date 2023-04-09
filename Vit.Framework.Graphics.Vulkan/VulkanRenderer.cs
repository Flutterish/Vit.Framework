using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanRenderer : Renderer {
	public readonly VulkanInstance Instance;

	public VulkanRenderer ( VulkanInstance instance, IEnumerable<RenderingCapabilities> capabilities ) : base( RenderingApi.Vulkan, capabilities ) {
		Instance = instance;
	}

	public override Matrix4<T> CreateLeftHandCorrectionMatrix<T> () {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}

	protected override void Dispose ( bool disposing ) {
		Instance.Dispose();
	}
}
