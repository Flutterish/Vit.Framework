using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public static unsafe class ShaderBindingExtensions {
	public static (VkVertexInputAttributeDescription[] attribs, VkVertexInputBindingDescription[] sets) GenerateVertexBindings ( this ShaderInfo shader ) {
		var sets = shader.BufferInput?.Sets ?? throw new Exception( "Shader had no buffer input defined" );

		var vertexAttributes = new VkVertexInputAttributeDescription[sets.Sum( x => x.Value.Resources.Count )];
		var vertexBindings = new VkVertexInputBindingDescription[sets.Count];

		uint i = 0;
		uint j = 0;
		foreach ( var (set, attribs) in sets ) {
			uint offset = 0;
			foreach ( var attrib in attribs.Resources ) {
				if ( attrib.Type.Layout != null )
					throw new Exception( "Input attributes cannot be structs" );

				var (format, size) = (attrib.Type.PrimitiveType, attrib.Type.Dimensions) switch {
					(PrimitiveType.Float32, [2]) => (VkFormat.R32g32Sfloat, sizeof(float)),
					(PrimitiveType.Float32, [3]) => (VkFormat.R32g32b32Sfloat, sizeof(float)),
					_ => throw new Exception( "Unrecognized format" )
				};
				size = attrib.Type.Dimensions.Aggregate( size, (a, b) => a * (int)b );

				vertexAttributes[i++] = new() {
					binding = set,
					location = attrib.Location,
					offset = offset,
					format = format
				};

				offset += (uint)size;
			}

			vertexBindings[j++] = new() {
				binding = set,
				inputRate = VkVertexInputRate.Vertex,
				stride = offset
			};
		}

		return (vertexAttributes, vertexBindings);
	}

	public static VkDescriptorSetLayoutBinding[] GenerateUniformBindings ( this IEnumerable<ShaderInfo> shaders ) {
		Dictionary<uint, (VkDescriptorType format, VkShaderStageFlags stages)> bindings = new();

		foreach ( var shader in shaders ) {
			var stage = ShaderModule.FlagsFromPartType( shader.Type );
			foreach ( var resource in shader.Uniforms.Resources.Concat( shader.Samplers.Resources ) ) {
				var binding = resource.Binding;
				var format = resource.Type switch {
					{ PrimitiveType: PrimitiveType.Struct } => VkDescriptorType.UniformBuffer,
					{ PrimitiveType: PrimitiveType.Sampler } => VkDescriptorType.CombinedImageSampler,
					_ => throw new Exception( "Unrecognized format" )
				};

				if ( bindings.TryGetValue( binding, out var info ) ) {
					if ( info.format != format )
						throw new Exception( "Uniform formats dont match across shader modules" );

					bindings[binding] = (format, stage | info.stages);
				}
				else {
					bindings.Add( binding, (format, stage) );
				}
			}
		}

		return bindings.Select( x => new VkDescriptorSetLayoutBinding() {
			binding = x.Key,
			descriptorCount = 1,
			descriptorType = x.Value.format,
			stageFlags = x.Value.stages
		} ).ToArray();
	}

	public static DescriptorPool CreateDesscriptorPool ( this VkDescriptorSetLayoutBinding[] layout, VkDevice device ) {
		var sizes = new VkDescriptorPoolSize[layout.Length];
		for ( int i = 0; i < sizes.Length; i++ ) {
			sizes[i] = new() {
				type = layout[i].descriptorType,
				descriptorCount = 1
			};
		}

		return new( device, sizes );
	}
}
