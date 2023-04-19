using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanRenderer : Renderer {
	public VulkanRenderer ( VulkanApi api ) : base( api ) {

	}

	public override Matrix4<T> CreateLeftHandCorrectionMatrix<T> () {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}
}
