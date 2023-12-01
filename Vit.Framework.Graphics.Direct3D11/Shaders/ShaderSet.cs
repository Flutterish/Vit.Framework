using System.Collections.Immutable;
using Vit.Framework.Graphics.Direct3D11.Uniforms;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
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

	const string TEXCOORD = "TEXCOORD";
	static string[] TEXCOORDN_ = new string[] {
		"TEXCOORD0_",
		"TEXCOORD1_",
		"TEXCOORD2_",
		"TEXCOORD3_"
	};

	public readonly ID3D11InputLayout? Layout;
	public ShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) {
		Shaders = parts.Select( x => (UnlinkedShader)x ).ToImmutableArray();

		var uniformInfo = this.CreateUniformInfo();
		var uniformMapping = uniformInfo.CreateFlatMapping();
		LinkedShaders = Shaders.Select( x => x.GetShader( uniformMapping ) ).ToImmutableArray();

		var sets = uniformInfo.Sets.Any() ? uniformInfo.Sets.Max( x => x.Key ) + 1 : 0;
		UniformLayouts = new UniformLayout[sets];
		UniformSets = new UniformSet[sets];
		for ( uint j = 0; j < sets; j++ ) {
			UniformLayouts[j] = new( j, uniformInfo.Sets.GetValueOrDefault( j ) ?? new(), uniformMapping );
		}

		if ( vertexInput == null )
			return;

		var bufferCount = vertexInput.BufferBindings.Any() ? vertexInput.BufferBindings.Max( x => x.Key ) + 1 : 0;
		BufferStrides = new int[bufferCount];

		var inputs = new InputElementDescription[vertexInput.BufferBindings.Sum( x => x.Value.AttributesByLocation.Sum( x => x.Value.Locations ))];
		var inputIndex = 0;
		foreach ( var (buffer, attributes) in vertexInput.BufferBindings ) {
			foreach ( var (location, attribute) in attributes.AttributesByLocation ) {
				var format = (attribute.DataType.PrimitiveType, attribute.DataType.Dimensions) switch {
					(PrimitiveType.UInt32, []) => Format.R32_UInt,
					(PrimitiveType.Float32, []) => Format.R32_Float,
					(PrimitiveType.Float32, [2, ..]) => Format.R32G32_Float,
					(PrimitiveType.Float32, [3, ..]) => Format.R32G32B32_Float,
					(PrimitiveType.Float32, [4, ..]) => Format.R32G32B32A32_Float,
					_ => throw new Exception( "Unrecognized format" )
				};

				for ( uint i = 0; i < attribute.Locations; i++ ) {
					if ( attribute.Locations == 1 ) {
						inputs[inputIndex++] = new() {
							SemanticName = TEXCOORD,
							SemanticIndex = (int)location,
							Format = format,
							Slot = (int)buffer,
							AlignedByteOffset = (int)attribute.Offset,
							Classification = attributes.InputRate == BufferInputRate.PerVertex ? InputClassification.PerVertexData : InputClassification.PerInstanceData,
							InstanceDataStepRate = attributes.InputRate == BufferInputRate.PerVertex ? 0 : 1
						};
					}
					else {
						inputs[inputIndex++] = new() {
							SemanticName = TEXCOORDN_[location],
							SemanticIndex = (int)i,
							Format = format,
							Slot = (int)buffer,
							AlignedByteOffset = (int)(attribute.Offset + i * attribute.LocationByteSize),
							Classification = attributes.InputRate == BufferInputRate.PerVertex ? InputClassification.PerVertexData : InputClassification.PerInstanceData,
							InstanceDataStepRate = attributes.InputRate == BufferInputRate.PerVertex ? 0 : 1
						};
					}
				}
			}

			BufferStrides[buffer] = (int)attributes.Stride;
		}

		var vert = LinkedShaders.OfType<VertexShader>().First();
		Layout = vert.Handle.Device.CreateInputLayout( inputs, vert.Source.Span );
	}

	public int[] BufferStrides = null!;

	public UniformLayout[] UniformLayouts;
	public UniformSet[] UniformSets;
	public IUniformSet? GetUniformSet ( uint set = 0 ) {
		return UniformSets[set];
	}

	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		var value = new UniformSet( UniformLayouts[set] );
		DebugMemoryAlignment.SetDebugData( value, UniformLayouts[set].Type.Resources );
		return value;
	}

	public IUniformSetPool CreateUniformSetPool ( uint set, uint size ) {
		return new UniformSetPool( UniformLayouts[set] );
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[set] = (UniformSet)uniforms;
	}

	protected override void Dispose ( bool disposing ) {
		Layout?.Dispose();
	}
}
