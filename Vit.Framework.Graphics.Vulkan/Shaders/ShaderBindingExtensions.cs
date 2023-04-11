using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public static unsafe class ShaderBindingExtensions {
	public static (VkVertexInputAttributeDescription[] attribs, VkVertexInputBindingDescription[] sets) GenerateVertexBindings ( this ShaderInfo shader ) {
		var vertexAttributes = new VkVertexInputAttributeDescription[shader.Input.Resources.Count];
		var vertexBindings = new VkVertexInputBindingDescription[1];

		uint i = 0;
		uint offset = 0;
		foreach ( var attrib in shader.Input.Resources ) {
			if ( attrib.Type.Layout != null )
				throw new Exception( "Input attributes cannot be structs" );

			var (format, size) = (attrib.Type.PrimitiveType, attrib.Type.Dimensions) switch {
				(PrimitiveType.Float32, [2]) => (VkFormat.R32g32Sfloat, sizeof( float )),
				(PrimitiveType.Float32, [3]) => (VkFormat.R32g32b32Sfloat, sizeof( float )),
				_ => throw new Exception( "Unrecognized format" )
			};
			size = attrib.Type.Dimensions.Aggregate( size, ( a, b ) => a * (int)b );

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

	public static Dictionary<uint, VkDescriptorSetLayoutBinding[]> GenerateUniformBindings ( this IEnumerable<ShaderInfo> shaders ) {
		var sets = shaders.SelectMany( x => new[] { x.Uniforms, x.Samplers } ).SelectMany( x => x.Sets.Keys ).Distinct();

		return sets.ToDictionary(
			x => x,
			x => shaders.GenerateUniformBindingsSet( x )
		);
	}

	public static VkDescriptorSetLayoutBinding[] GenerateUniformBindingsSet ( this IEnumerable<ShaderInfo> shaders, uint set ) {
		Dictionary<uint, (VkDescriptorType format, VkShaderStageFlags stages)> bindings = new();

		foreach ( var shader in shaders ) {
			var stage = ShaderModule.FlagsFromPartType( shader.Type );
			IEnumerable<UniformResourceInfo> resources = Enumerable.Empty<UniformResourceInfo>();
			if ( shader.Uniforms.Sets.TryGetValue( set, out var setInfo ) )
				resources = resources.Concat( setInfo.Resources );
			if ( shader.Samplers.Sets.TryGetValue( set, out setInfo ) )
				resources = resources.Concat( setInfo.Resources );

			foreach ( var resource in resources ) {
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
