using System.Collections.Concurrent;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Shaders;

public class ShaderStore : DisposableObject {
	ConcurrentQueue<ShaderPart> partsToCompile = new();
	ConcurrentQueue<Shader> shadersToCompile = new();

	Dictionary<ShaderIdentifier, ShaderPart> shaderParts = new();

	public void AddShaderPart ( ShaderIdentifier id, ShaderPart part ) {
		shaderParts.Add( id, part );
		partsToCompile.Enqueue( part );
	}

	public void AddShaderPart ( ShaderIdentifier id, SpirvBytecode part ) {
		AddShaderPart( id, new ShaderPart( part ) );
	}

	Dictionary<ShaderDescription, Shader> shaders = new();
	public Shader GetShader ( ShaderDescription description ) {
		if ( !shaders.TryGetValue( description, out var shader ) ) {
			shader = new( new[] { shaderParts[description.Vertex!], shaderParts[description.Fragment!] } );
			shaders.Add( description, shader );
			shadersToCompile.Enqueue( shader );
		}

		return shader;
	}

	public void CompileNew ( IRenderer renderer ) { // TODO this is not correct. a shader can be added while its parts are not compiled.
		while ( partsToCompile.TryDequeue( out var part ) ) {
			part.Compile( renderer );
		}

		while ( shadersToCompile.TryDequeue( out var shader ) ) {
			shader.Compile( renderer );
		}
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, i) in shaders ) {
			i.Dispose();
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
	public ShaderIdentifier? Vertex;
	public ShaderIdentifier? Fragment;
}