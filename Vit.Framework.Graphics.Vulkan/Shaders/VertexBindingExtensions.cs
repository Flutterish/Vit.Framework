using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public static unsafe class VertexBindingExtensions {
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
}
