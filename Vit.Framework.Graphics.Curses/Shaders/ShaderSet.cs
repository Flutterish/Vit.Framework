using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Software.Buffers;

namespace Vit.Framework.Graphics.Curses.Shaders;

public class ShaderSet : IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<Shader> Shaders;
	public ShaderSet ( IEnumerable<IShaderPart> parts ) {
		Shaders = parts.Select( x => (Shader)x ).ToImmutableArray();
	}

	public readonly Dictionary<uint, (IByteBuffer buffer, uint stride, uint offset)> UniformBuffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		var vertex = Shaders.First( x => x.Type == ShaderPartType.Vertex ).SoftwareShader;
		UniformBuffers[binding] = ((IByteBuffer)buffer, IBuffer<T>.Stride, offset * IBuffer<T>.Stride);
	}

	public void Dispose () {
		
	}
}
