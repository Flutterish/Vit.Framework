using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vit.Framework.Memory.Allocation;

/// <summary>
/// A heap allocator whose metadata is stored outside of the memory owned by it.
/// </summary>
public class DecoupledHeapAllocator {
	nuint totalSize;
	public DecoupledHeapAllocator ( nuint size, nuint? expectedAllocationSize = null ) {
		totalSize = size;
		if ( expectedAllocationSize is nuint expected ) {
			var expectedCount = (int)((size + expected - 1) / expected);

			regionsByAddress = new( expectedCount );
			metadataPool = new( expectedCount );
		}
		else {
			regionsByAddress = new();
			metadataPool = new();
		}

		var bucketIndex = getBucketIndex( size );
		freeBuckets = new RegionMetadata?[bucketIndex + 1];
		var header = getNewMetadata( 0 );
		header.IsFree = true;
		header.Size = size;
		freeBuckets[bucketIndex] = header;
		freeBucketCount = bucketIndex + 1;

		// fake allocated region after all data
		var end = getNewMetadata( size );
		end.IsFree = false;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	static nuint getBucketIndex ( nuint size ) {
		return nuint.Log2( size );
	}

	/// <inheritdoc cref="IAllocator.Allocate(nuint)"/>
	public unsafe DecoupledAllocation Allocate ( nuint size ) {
		Debug.Assert( size != 0 );

		RegionMetadata? node = null;
		var bucketIndex = getBucketIndex( size );
		for ( ; bucketIndex < freeBucketCount; bucketIndex++ ) {
			node = freeBuckets[bucketIndex];

			while ( node != null && node.Size < size ) {
				node = node.NextFreeBySize;
			}

			if ( node != null )
				break;
		}

		if ( node == null )
			return new( 0, 0 );

		// marking the node as not free
		node.IsFree = false;
		removeNode( node, bucketIndex );

		if ( node.Size != size ) {
			// we have to split into 2 blocks
			var remaining = node.Size - size;
			node.Size = size;

			var newNode = getNewMetadata( node.Offset + size );
			newNode.IsFree = true;
			newNode.Size = remaining;
			newNode.Previous = node;

			var nextNode = regionsByAddress[newNode.Offset + newNode.Size];
			nextNode.Previous = newNode;

			insertNode( newNode );
		}

		return new( node.Offset, size );
	}

	void split ( RegionMetadata node, nuint size ) {
		var remaining = node.Size - size;
		node.Size = size;

		var newNode = getNewMetadata( node.Offset + node.Size );
		newNode.IsFree = true;
		newNode.Size = remaining;
		newNode.Previous = node;

		var nextNode = regionsByAddress[newNode.Offset + newNode.Size];
		nextNode.Previous = newNode;

		insertNode( newNode );
	}

	void removeNode ( RegionMetadata node, nuint bucketIndex ) {
		if ( freeBuckets[bucketIndex] == node ) {
			freeBuckets[bucketIndex] = node.NextFreeBySize;
		}

		if ( node.PreviousFreeBySize != null )
			node.PreviousFreeBySize.NextFreeBySize = node.NextFreeBySize;
		if ( node.NextFreeBySize != null )
			node.NextFreeBySize.PreviousFreeBySize = node.PreviousFreeBySize;
	}

	void insertNode ( RegionMetadata node ) {
		var bucketIndex = getBucketIndex( node.Size );
		var bucketNode = freeBuckets[bucketIndex];

		if ( bucketNode == null ) {
			freeBuckets[bucketIndex] = node;
			node.NextFreeBySize = null;
			node.PreviousFreeBySize = null;
		}
		else if ( bucketNode.Size >= node.Size ) {
			// we are now the first item in the bucket
			freeBuckets[bucketIndex] = node;
			node.NextFreeBySize = bucketNode;
			node.PreviousFreeBySize = bucketNode.PreviousFreeBySize;

			bucketNode.PreviousFreeBySize = node;
		}
		else {
			// we need to sort outselves
			while ( bucketNode.NextFreeBySize != null && bucketNode.NextFreeBySize.Size < node.Size ) {
				bucketNode = bucketNode.NextFreeBySize;
			}

			if ( bucketNode.NextFreeBySize == null ) {
				// we are the biggest one
				bucketNode.NextFreeBySize = node;
				node.PreviousFreeBySize = bucketNode;
				node.NextFreeBySize = null;
			}
			else {
				// bucketNode->NextFreeBySize has bigger or equal size
				// and bucketNode has smaller size
				node.PreviousFreeBySize = bucketNode;
				node.NextFreeBySize = bucketNode.NextFreeBySize;
				bucketNode.NextFreeBySize.PreviousFreeBySize = node;
				bucketNode.NextFreeBySize = node;
			}
		}
	}

	/// <summary>
	/// Attempts to expand or shrink allocated memory in place to <c><paramref name="newSize"/></c> bytes.
	/// </summary>
	/// <param name="moved"><see langword="true"/> if the pointer was moved to a new place (the data is not copied as this is a decoupled allocator), <see langword="false"/> if the operation was in place.</param>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory, in which case this is equivalent to a <see cref="Free(nuint)"/>.
	/// </remarks>
	public unsafe DecoupledAllocation Reallocate ( nuint ptr, nuint newSize, out bool moved ) {
		Debug.Assert( newSize != 0 );
		RegionMetadata node = regionsByAddress[ptr];

		var oldSize = node.Size;
		removeNode( node, getBucketIndex( oldSize ) );

		// basically free this node and merge it with next (if possible)
		var next = regionsByAddress[node.Offset + node.Size];
		if ( next.IsFree ) {
			removeNode( next, getBucketIndex( next.Size ) );
			regionsByAddress[next.Offset + next.Size].Previous = node;
			node.Size += next.Size;
			freeMetadata( next );
		}

		bool tryFit () {
			// if it fits, I sits (this is also good for shrinks)
			if ( node.Size == newSize ) {
				insertNode( node );
				return true;
			}
			else if ( node.Size > newSize ) {
				// split into 2
				split( node, newSize );
				insertNode( node );
			}

			return false;
		}

		if ( tryFit() ) {
			moved = false;
			return new( ptr, newSize );
		}

		// otherwise, try to merge with previous and do the same check
		moved = true;

		var previous = node.Previous;
		if ( previous != null && previous.IsFree ) {
			removeNode( previous, getBucketIndex( previous.Size ) );
			regionsByAddress[node.Offset + node.Size].Previous = previous;
			previous.Size += node.Size;
			freeMetadata( node );
			node = previous;
		}

		if ( tryFit() ) {
			node.IsFree = false;
			// this would be a memcpy if we had access to the memory

			return new( node.Offset, newSize );
		}

		// if that fails too, we basically already freed this block, so we can just allocate a new one and move the data there
		// the edge case is when we are out of memory, in which case we have just freed the memory
		var allocation = Allocate( newSize );
		if ( allocation.Bytes != newSize ) {
			return new( 0, 0 );
		}
		// this would be a memcpy if we had access to the memory
		node.IsFree = true;
		return allocation;
	}

	/// <inheritdoc cref="IAllocator.Free(void*)"/>
	public unsafe void Free ( nuint ptr ) {
		var node = regionsByAddress[ptr];
		node.IsFree = true;

		var previous = node.Previous;
		var next = regionsByAddress[node.Offset + node.Size];

		if ( next.IsFree ) {
			removeNode( next, getBucketIndex( next.Size ) );
			regionsByAddress[next.Offset + next.Size].Previous = node;
			node.Size += next.Size;
			next.Previous = null;
			next.NextFreeBySize = null;
			freeMetadata( next );
		}

		if ( previous != null && previous.IsFree ) {
			removeNode( previous, getBucketIndex( previous.Size ) );
			regionsByAddress[node.Offset + node.Size].Previous = previous;
			previous.Size += node.Size;

			node.Previous = null;
			freeMetadata( node );
			node = previous;
		}

		insertNode( node );
	}

	static string[] sizes = new[] { "B", "kiB", "MiB", "GiB" };
	static string formatSize ( double size ) {
		int index = 0;
		while ( index + 1 < sizes.Length && size > 1024 * 1.5 ) {
			size /= 1024;
			index++;
		}

		return $"{size:N1}{sizes[index]}";
	}

	public override string ToString () {
		StringBuilder sb = new();
		sb.AppendLine( "Sequential View:" );
		var head = regionsByAddress[0];
		var tail = regionsByAddress[totalSize];

		while ( head != tail ) {
			sb.AppendLine( $"\t[{(head.IsFree ? "Free" : "Taken")} {formatSize( head.Size )}]" );
			head = regionsByAddress[head.Offset + head.Size];
		}
		sb.AppendLine();
		sb.AppendLine( "Free Bucket View:" );
		for ( nuint i = 0; i < freeBucketCount; i++ ) {
			sb.Append( $"\tBucket {i} [{formatSize( Math.Pow( 2, i ) )} to {formatSize( Math.Pow( 2, i + 1 ) )}]: " );
			bool isfirst = true;
			head = freeBuckets[i];
			if ( head == null )
				sb.Append( "Empty" );

			while ( head != null ) {
				if ( !isfirst ) {
					sb.Append( " -> " );
				}

				sb.Append( $"{formatSize( head.Size )}" );
				head = head.NextFreeBySize;

				isfirst = false;
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	Stack<RegionMetadata> metadataPool;
	RegionMetadata getNewMetadata ( nuint address ) {
		if ( !metadataPool.TryPop( out var region ) )
			region = new();

		region.Offset = address;
		regionsByAddress[address] = region;
		return region;
	}
	void freeMetadata ( RegionMetadata region ) {
		regionsByAddress.Remove( region.Offset );
		Debug.Assert( region.NextFreeBySize == null );
		Debug.Assert( region.PreviousFreeBySize == null );
		Debug.Assert( region.Previous == null );
		metadataPool.Push( region );
	}
	Dictionary<nuint, RegionMetadata> regionsByAddress;
	nuint freeBucketCount;
	// the first free nodes of size [2^index; 2^(index + 1)). also, nodes are only linked by size within buckets
	RegionMetadata?[] freeBuckets;

	class RegionMetadata {
		public bool IsFree;
		public nuint Offset;
		public nuint Size;

		// sorted linked list, by size. only links to other nodes in the same bucket
		public RegionMetadata? PreviousFreeBySize;
		public RegionMetadata? NextFreeBySize;

		// metadata about physicaly adjacent regions
		public RegionMetadata? Previous;
	}
}

/// <summary>
/// A decoupled allocation. The pointer value may by 0 and still valid, however a 0-length allocation is invalid.
/// </summary>
public struct DecoupledAllocation {
	public nuint Pointer;
	public nuint Bytes;

	public DecoupledAllocation ( nuint pointer, nuint bytes ) {
		Pointer = pointer;
		Bytes = bytes;
	}

	public void Deconstruct ( out nuint ptr, out nuint bytes ) {
		ptr = Pointer;
		bytes = Bytes;
	}

	public static implicit operator nuint ( DecoupledAllocation allocation )
		=> allocation.Pointer;
}