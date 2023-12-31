﻿using SPIRVCross;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public static unsafe class ShaderBindingExtensions {
	public static (VkVertexInputAttributeDescription[] attribs, VkVertexInputBindingDescription[] bindings) GenerateVertexBindings ( this VertexInputDescription vertexInput ) {
		var attribs = new VkVertexInputAttributeDescription[vertexInput.BufferBindings.Sum( x => x.Value.AttributesByLocation.Sum( x => x.Value.Locations ) )];
		var bindings = new VkVertexInputBindingDescription[vertexInput.BufferBindings.Count];

		uint bindingIndex = 0;
		uint attributeIndex = 0;
		foreach ( var (buffer, attributes) in vertexInput.BufferBindings ) {
			foreach ( var (location, attribute) in attributes.AttributesByLocation ) {
				var format = (attribute.DataType.PrimitiveType, attribute.DataType.Dimensions) switch {
					(PrimitiveType.UInt32, [] ) => VkFormat.A8b8g8r8UintPack32,
					(PrimitiveType.Float32, [] ) => VkFormat.R32Sfloat,
					(PrimitiveType.Float32, [2, ..] ) => VkFormat.R32g32Sfloat,
					(PrimitiveType.Float32, [3, ..] ) => VkFormat.R32g32b32Sfloat,
					(PrimitiveType.Float32, [4, ..] ) => VkFormat.R32g32b32a32Sfloat,
					_ => throw new Exception( "Unrecognized format" )
				};

				for ( uint i = 0; i < attribute.Locations; i++ ) {
					attribs[attributeIndex++] = new() {
						binding = buffer,
						location = location + i,
						offset = attribute.Offset + i * attribute.LocationByteSize,
						format = format
					};
				}
			}

			bindings[bindingIndex++] = new() {
				binding = buffer,
				inputRate = attributes.InputRate == BufferInputRate.PerVertex ? VkVertexInputRate.Vertex : VkVertexInputRate.Instance,
				stride = attributes.Stride
			};
		}

		return (attribs, bindings);
	}

	public static VkDescriptorSetLayoutBinding[] GenerateUniformBindingsSet ( this UniformSetInfo shader ) {
		Dictionary<uint, (VkDescriptorType format, VkShaderStageFlags stages)> bindings = new();

		foreach ( var resource in shader.Resources ) {
			var binding = resource.Binding;
			var stage = resource.Stages.Aggregate( VkShaderStageFlags.None, (a,b) => a | ShaderModule.FlagsFromPartType(b) );
			var format = resource.ResourceType switch { 
				spvc_resource_type.StorageBuffer => VkDescriptorType.StorageBuffer,
				spvc_resource_type.UniformBuffer => VkDescriptorType.UniformBuffer, 
				spvc_resource_type.SampledImage => VkDescriptorType.CombinedImageSampler,
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
}
