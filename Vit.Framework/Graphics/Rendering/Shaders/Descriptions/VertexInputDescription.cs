using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;

namespace Vit.Framework.Graphics.Rendering.Shaders.Descriptions;

public record VertexAttributeDescription {
	/// <summary>
	/// Byte offset within the containing buffer.
	/// </summary>
	public required uint Offset { get; init; }
	public required DataTypeInfo DataType { get; init; }

	public override string ToString () {
		return $"{{ Offset = {Offset}, DataType = {DataType} }}";
	}
}

public enum BufferInputRate {
	PerVertex,
	PerInstance
}

public record VertexBufferDescription {
	/// <summary>
	/// Stride of the buffer, in bytes. This is the amount of memory (including padding) that one set of attributes takes.
	/// </summary>
	public required uint Stride { get; init; }
	/// <summary>
	/// Whether the attributes change per vertex or per instance.
	/// </summary>
	public required BufferInputRate InputRate { get; init; }
	public required ImmutableDictionary<uint, VertexAttributeDescription> AttributesByLocation { get; init; }

	public override string ToString () {
		return $"{{ Stride = {Stride}, InputRate = {InputRate}, Attributes = [\n\t{string.Join(",\n\t", AttributesByLocation.Select( 
			x => $"[Location {x.Key}] = {x.Value}"
		))}\n] }}";
	}
}

public record VertexInputDescription {
	public required ImmutableDictionary<uint, VertexBufferDescription> BufferBindings { get; init; }

	/// <summary>
	/// Creates an input description for a shader containing one sequential vertex buffer.
	/// </summary>
	public static VertexInputDescription CreateSingle ( ShaderInfo shader ) {
		Dictionary<uint, VertexAttributeDescription> attributes = new( shader.Input.Resources.Count );

		uint i = 0;
		uint offset = 0;
		foreach ( var attrib in shader.Input.Resources.OrderBy( x => x.Location ) ) {
			var size = attrib.Type.PrimitiveType.SizeOf() * attrib.Type.FlattendedDimensions;

			attributes.Add( i++, new() {
				DataType = attrib.Type,
				Offset = offset
			} );

			offset += size;
		}

		Dictionary<uint, VertexBufferDescription> buffers = new( 1 );
		buffers.Add( 0, new() { 
			Stride = offset,
			InputRate = BufferInputRate.PerVertex,
			AttributesByLocation = attributes.ToImmutableDictionary()
		} );

		return new() { 
			BufferBindings = buffers.ToImmutableDictionary()
		};
	}

	/// <summary>
	/// Creates an input description for a shader containing one vertex buffer per attribute.
	/// </summary>
	public static VertexInputDescription CreateSeparate ( ShaderInfo shader ) {
		Dictionary<uint, VertexBufferDescription> buffers = new( shader.Input.Resources.Count );

		uint i = 0;
		foreach ( var attrib in shader.Input.Resources.OrderBy( x => x.Location ) ) {
			Dictionary<uint, VertexAttributeDescription> attributes = new( 1 );
			attributes.Add( attrib.Location, new() {
				Offset = 0,
				DataType = attrib.Type
			} );

			var size = attrib.Type.PrimitiveType.SizeOf() * attrib.Type.FlattendedDimensions;
			buffers.Add( i++, new() {
				InputRate = BufferInputRate.PerVertex,
				Stride = size,
				AttributesByLocation = attributes.ToImmutableDictionary()
			} );
		}

		return new() {
			BufferBindings = buffers.ToImmutableDictionary()
		};
	}

	public override string ToString () {
		return $"{{\n\t{string.Join(",\n\t", BufferBindings.Select( 
			x => $"[Buffer {x.Key}] -> {x.Value.ToString().ReplaceLineEndings("\n\t")}"
		))}\n}}";
	}
}