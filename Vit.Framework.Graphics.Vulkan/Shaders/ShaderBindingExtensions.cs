using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Vulkan.Uniforms;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public static unsafe class ShaderBindingExtensions {
	public static (VkVertexInputAttributeDescription[] attribs, VkVertexInputBindingDescription[] sets) GenerateVertexBindings ( this ShaderInfo shader ) {
		var vertexAttributes = new VkVertexInputAttributeDescription[shader.Input.Resources.Count];
		var vertexBindings = new VkVertexInputBindingDescription[1];

		uint i = 0;
		uint offset = 0;
		foreach ( var attrib in shader.Input.Resources.OrderBy( x => x.Location ) ) {
			if ( attrib.Type.Layout != null )
				throw new Exception( "Input attributes cannot be structs" );

			var (format, size) = (attrib.Type.PrimitiveType, attrib.Type.Dimensions) switch {
				(PrimitiveType.Float32, [2]) => (VkFormat.R32g32Sfloat, sizeof( float )),
				(PrimitiveType.Float32, [3]) => (VkFormat.R32g32b32Sfloat, sizeof( float )),
				_ => throw new Exception( "Unrecognized format" )
			};
			size *= (int)attrib.Type.FlattendedDimensions;

			vertexAttributes[i++] = new() {
				binding = 0,
				location = attrib.Location,
				offset = offset,
				format = format
			};

			offset += (uint)size;
		}

		vertexBindings[0] = new() {
			binding = 0,
			inputRate = VkVertexInputRate.Vertex,
			stride = offset
		};

		return (vertexAttributes, vertexBindings);
	}

	public static VkDescriptorSetLayoutBinding[] GenerateUniformBindingsSet ( this UniformSetInfo shader ) {
		Dictionary<uint, (VkDescriptorType format, VkShaderStageFlags stages)> bindings = new();

		foreach ( var resource in shader.Resources ) {
			var binding = resource.Binding;
			var stage = resource.Stages.Aggregate( VkShaderStageFlags.None, (a,b) => a | ShaderModule.FlagsFromPartType(b) );
			var format = resource.Type switch { 
				{ PrimitiveType: PrimitiveType.Struct } => VkDescriptorType.UniformBuffer, 
				{ PrimitiveType: PrimitiveType.Sampler } => VkDescriptorType.CombinedImageSampler,
				_ => throw new Exception( "Unrecognized format" )
			};

			bindings.Add( binding, (format, stage) );
		}

		return bindings.Select( x => new VkDescriptorSetLayoutBinding() {
			binding = x.Key,
			descriptorCount = 1,
			descriptorType = x.Value.format,
			stageFlags = x.Value.stages
		} ).ToArray();
	}

	public static DescriptorPool CreateDescriptorPool ( this VkDescriptorSetLayoutBinding[] layout, Device device, uint size = 1 ) {
		var sizes = new VkDescriptorPoolSize[layout.Length];
		for ( int i = 0; i < sizes.Length; i++ ) {
			sizes[i] = new() {
				type = layout[i].descriptorType,
				descriptorCount = 1
			};
		}

		return new DescriptorPool( device, size, layout, sizes );
	}
}
