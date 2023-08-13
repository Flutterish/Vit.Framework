using SPIRVCross;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class UniformInfo {
	static readonly spvc_resource_type[] resourceTypes = new[] { spvc_resource_type.UniformBuffer, spvc_resource_type.SampledImage, spvc_resource_type.StorageBuffer };
	public readonly Dictionary<uint, UniformSetInfo> Sets = new();

	public unsafe void ParseSpirv ( spvc_compiler compiler, spvc_resources resources ) {
		spvc_reflected_resource* list = default;
		nuint count = default;

		foreach ( var resourceType in resourceTypes ) {
			SPIRV.spvc_resources_get_resource_list_for_type( resources, resourceType, (spvc_reflected_resource*)&list, &count );
			for ( nuint i = 0; i < count; i++ ) {
				var res = list[i];
				var set = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)res.id, SpvDecoration.SpvDecorationDescriptorSet );

				if ( !Sets.TryGetValue( set, out var setInfo ) )
					Sets.Add( set, setInfo = new() );

				var resource = new UniformResourceInfo();
				resource.ResourceType = resourceType;
				resource.ParseSpriv( compiler, res );
				setInfo.Resources.Add( resource );
			}
		}
	}

	public override string ToString () {
		return $"<\n\t{string.Join( "\n", Sets.Select( x => $"Set {x.Key} {x.Value}" ) ).Replace( "\n", "\n\t" )}\n>";
	}

	/// <summary>
	/// Creates a flat mapping of uniforms (for backends that do not support uniform sets). <br/>
	/// The resulting mapping's resources are contigious by resource type globally (within a set each type is contigious too).
	/// </summary>
	/// <remarks>
	/// Example mapping: <c>[unifrom buffers[set 0[1, 2]; set 1[3, 4, 5]]; samplers[set 0[6]; set 1[7, 8]]</c>
	/// </remarks>
	public UniformFlatMapping CreateFlatMapping () {
		UniformFlatMapping mapping = new();

		uint nextBinding = 0;
		foreach ( var resourceType in resourceTypes ) {
			foreach ( var (set, setInfo) in Sets ) {
				foreach ( var i in setInfo.Resources.Where( x => x.ResourceType == resourceType ) ) {
					mapping.Bindings.Add( (set, i.Binding), nextBinding++ );
				}
			}
		}

		return mapping;
	}
}

public class UniformFlatMapping {
	public Dictionary<(uint set, uint binding), uint> Bindings = new();

	public bool StructuralEquals ( UniformFlatMapping other ) {
		if ( Bindings.Count != other.Bindings.Count )
			return false;

		foreach ( var ((set, binding), mapped) in Bindings ) {
			if ( !other.Bindings.TryGetValue( (set, binding), out var otherMapped ) || otherMapped != mapped )
				return false;
		}

		return true;
	}

	public void Apply ( SpirvBytecode spirv, Span<byte> data ) {
		var wordView = MemoryMarshal.Cast<byte, uint>( data );
		foreach ( var ((set, originalBinding), binding) in Bindings ) {
			if ( !spirv.Reflections.Uniforms.Sets.TryGetValue( set, out var setInfo ) || setInfo.Resources.FirstOrDefault( x => x.Binding == originalBinding ) is not UniformResourceInfo resource )
				continue;

			wordView[(int)resource.BindingBinaryOffset] = binding;
		}
	}
}

public class UniformFlatMappingDictionary<T> : IEnumerable<KeyValuePair<UniformFlatMapping, T>> where T : notnull {
	Dictionary<int, List<(UniformFlatMapping mapping, T value)>> lookup = new();
	
	int hash ( UniformFlatMapping mapping ) {
		var hash = 0;
		foreach ( var i in mapping.Bindings ) {
			hash = HashCode.Combine( hash, i.Value );
		}

		return hash;
	}

	public void Add ( UniformFlatMapping mapping, T value ) {
		var hash = this.hash( mapping );
		if ( !lookup.TryGetValue( hash, out var list ) ) {
			lookup.Add( hash, list = new() );
		}

		list.Add( (mapping, value) );
	}

	public bool TryGetValue ( UniformFlatMapping mapping, [NotNullWhen(true)] out T? value ) {
		var hash = this.hash( mapping );
		if ( !lookup.TryGetValue( hash, out var list ) ) {
			value = default;
			return false;
		}

		foreach ( var (map, v) in list ) {
			if ( map.StructuralEquals( mapping ) ) {
				value = v;
				return true;
			}
		}

		value = default;
		return false;
	}

	public IEnumerator<KeyValuePair<UniformFlatMapping, T>> GetEnumerator () {
		foreach ( var (_, list) in lookup ) {
			foreach ( var (k, v) in list ) {
				yield return new(k, v);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator () {
		return GetEnumerator();
	}
}