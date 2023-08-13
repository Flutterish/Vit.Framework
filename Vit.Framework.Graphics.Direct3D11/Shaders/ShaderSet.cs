using System.Collections.Immutable;
using Vit.Framework.Graphics.Direct3D11.Uniforms;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Shaders;

public class ShaderSet : DisposableObject, IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<UnlinkedShader> Shaders;
	public readonly ImmutableArray<Shader> LinkedShaders;

	public readonly ID3D11InputLayout? Layout;
	public readonly ID3D11DeviceContext Context;
	public readonly int Stride;
	public readonly UniformFlatMapping UniformMapping;
	public ShaderSet ( IEnumerable<IShaderPart> parts, ID3D11DeviceContext context ) {
		Context = context;
		Shaders = parts.Select( x => (UnlinkedShader)x ).ToImmutableArray();

		UniformMapping = this.CreateUniformInfo().CreateFlatMapping();
		LinkedShaders = Shaders.Select( x => x.GetShader( UniformMapping ) ).ToImmutableArray();

		var vert = LinkedShaders.OfType<VertexShader>().FirstOrDefault();
		if ( vert is null )
			return;

		var vertInfo = Shaders.First( x => x.Type == ShaderPartType.Vertex );
		var inputs = new InputElementDescription[vertInfo.ShaderInfo.Input.Resources.Count];
		int i = 0;
		int offset = 0;
		foreach ( var vertex in vertInfo.ShaderInfo.Input.Resources.OrderBy( x => x.Location ) ) {
			var (size, format) = (vertex.Type.PrimitiveType, vertex.Type.Dimensions) switch {
				(PrimitiveType.Float32, [2]) => (sizeof(float), Format.R32G32_Float),
				(PrimitiveType.Float32, [3]) => (sizeof(float), Format.R32G32B32_Float),
				_ => throw new Exception( "Unrecognized format" )
			};
			size *= (int)vertex.Type.FlattendedDimensions;

			inputs[i++] = new() {
				SemanticName = "TEXCOORD",
				SemanticIndex = (int)vertex.Location,
				Format = format,
				Slot = 0,
				AlignedByteOffset = offset,
				Classification = InputClassification.PerVertexData,
				InstanceDataStepRate = 0
			};

			offset += size;
		}

		Stride = offset;
		Layout = vert.Handle.Device.CreateInputLayout( inputs, vert.Source.Span );
	}

	public Dictionary<uint, UniformSet> UniformSets = new();
	public IUniformSet? GetUniformSet ( uint set = 0 ) {
		return UniformSets.GetValueOrDefault( set );
	}

	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		var value = new UniformSet();
		DebugMemoryAlignment.SetDebugData( value, set, this );
		return value;
	}

	public IUniformSetPool CreateUniformSetPool ( uint set, uint size ) {
		return new UniformSetPool( this.CreateUniformSetInfo( set ) );
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[set] = (UniformSet)uniforms;
	}

	protected override void Dispose ( bool disposing ) {
		Layout?.Dispose();
	}
}
