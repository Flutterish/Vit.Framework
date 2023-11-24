using System.Collections.Concurrent;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;

namespace Vit.Framework.Graphics.Shaders;

public class ShaderStore : IDisposable {
	ConcurrentQueue<Shader> shadersToCompile = new();
	Dictionary<ShaderIdentifier, ShaderPart> shaderParts = new();

	public void AddShaderPart ( ShaderIdentifier id, ShaderPart part ) {
		shaderParts.Add( id, part );
	}

	public void AddShaderPart ( ShaderIdentifier id, SpirvBytecode part ) {
		AddShaderPart( id, new ShaderPart( part ) );
	}

	Dictionary<ShaderDescription, Shader> shaders = new();
	public Shader GetShader ( ShaderDescription description ) {
		if ( !shaders.TryGetValue( description, out var shader ) ) {
			shader = new( new[] { shaderParts[description.Vertex!.Value.Shader], shaderParts[description.Fragment!] }, description.Vertex!.Value.Input );
			shaders.Add( description, shader );
			shadersToCompile.Enqueue( shader );
		}

		return shader;
	}

	public void CompileNew ( IRenderer renderer ) {
		while ( shadersToCompile.TryDequeue( out var shader ) ) {
			shader.Compile( renderer );
		}
	}

	public void Dispose () {
		foreach ( var (_, i) in shaders ) {
			i.Dispose();
			shadersToCompile.Enqueue( i );
		}

		foreach ( var (_, i) in shaderParts ) {
			i.Dispose();
		}
	}
}

public class ShaderIdentifier {
	public required string Name;
}

public struct ShaderDescription {
	public VertexShaderDescription? Vertex;
	public ShaderIdentifier? Fragment;
}

public struct VertexShaderDescription {
	public required ShaderIdentifier Shader;
	public required VertexInputDescription Input;
}