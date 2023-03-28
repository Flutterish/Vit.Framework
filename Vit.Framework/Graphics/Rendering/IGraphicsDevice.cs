using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Synchronisation;

namespace Vit.Framework.Graphics.Rendering;

public interface IGraphicsDevice : IDisposable {
	IGpuBarrier CreateGpuBarrier ();
	ICpuBarrier CreateCpuBarrier ( bool signaled = false );

	IShaderPart CreateShaderPart ( SpirvBytecode spirv );
	IShader CreateShader ( IShaderPart[] parts );
}
