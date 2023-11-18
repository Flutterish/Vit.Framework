using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vit.Framework.Interop;

namespace Vit.Framework.Memory.Allocation;

public unsafe class HeapAllocator : IAllocator {
	Header* @base;
	nuint totalSize;
	public HeapAllocator ( void* @base, nuint size ) {
		this.@base = (Header*)@base;
		totalSize = size;

		this.@base->NextFreeBySize = null;
		this.@base->PreviousFreeBySize = null;
		this.@base->Previous = null;
		this.@base->Size = size - SizeOfHelper<Header>.Size - SizeOfHelper<Header>.Size;
		this.@base->IsFree = true;
#if DEBUG_ALLOCATORS
		this.@base->Magic = Header.MagicValue;
#endif
		Header.GetNext( this.@base )->IsFree = false; // fake allocated region after all data

		var bucket = getBucketIndex( this.@base->Size );
		freeBuckets = new Header*[bucket + 1];
		freeBucketCount = (nuint)freeBuckets.Length;
		Array.Clear( freeBuckets );

		freeBuckets[bucket] = this.@base;
	}

	nuint freeBucketCount;
	// the first free nodes of size [2^index; 2^(index + 1)). also, nodes are only linked by size within buckets
	Header*[] freeBuckets; // also maybe put this in the data itself

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	static nuint getBucketIndex ( nuint size ) {
		return nuint.Log2( size );
	}

	public Allocation Allocate ( nuint size ) {
		Header* node = null;
		nuint bucketIndex = getBucketIndex( size );
		for ( ; bucketIndex < freeBucketCount; bucketIndex++ ) {
			node = freeBuckets[bucketIndex];
			// equal size means we dont need to create a header for the remaining space
			while ( node != null && node->Size != size && node->Size < size + SizeOfHelper<Header>.Size ) {
				node = node->NextFreeBySize;
			}

			if ( node != null )
				break;
		}

		if ( node == null )
			return new( null, 0 );

		// marking the node as not free
		node->IsFree = false;
		removeNode( node, bucketIndex );

		if ( node->Size != size ) {
			// we have to split into 2 blocks
			var remaining = node->Size - size;

			node->Size = size;

			var newNode = Header.GetNext( node );
			newNode->IsFree = true;
			newNode->Size = remaining - SizeOfHelper<Header>.Size;
			newNode->Previous = node;

			var nextNode = Header.GetNext( newNode );
			nextNode->Previous = newNode;

			insertNode( newNode );
#if DEBUG_ALLOCATORS
			newNode->Magic = Header.MagicValue;
#endif
		}

		return new( Header.Data( node ), size );
	}

	void split ( Header* node, nuint size ) {
		var remaining = node->Size - size;

		node->Size = size;

		var newNode = Header.GetNext( node );
		newNode->IsFree = true;
		newNode->Size = remaining - SizeOfHelper<Header>.Size;
		newNode->Previous = node;

		var nextNode = Header.GetNext( newNode );
		nextNode->Previous = newNode;

		insertNode( newNode );
#if DEBUG_ALLOCATORS
		newNode->Magic = Header.MagicValue;
#endif
	}

	void removeNode ( Header* node, nuint bucketIndex ) {
		if ( freeBuckets[bucketIndex] == node ) { // if it is in the bucket, it is the smallest one of that size group, so the next free one will replace it
			freeBuckets[bucketIndex] = node->NextFreeBySize;
		}

		if ( node->PreviousFreeBySize != null ) // TODO can these checks be eliminated if we have a dummy, 0-size, non-free node at start/end?
			node->PreviousFreeBySize->NextFreeBySize = node->NextFreeBySize;
		if ( node->NextFreeBySize != null )
			node->NextFreeBySize->PreviousFreeBySize = node->PreviousFreeBySize;
#if DEBUG_ALLOCATORS
		node->NextFreeBySize = null;
		node->PreviousFreeBySize = null;
#endif
	}

	void insertNode ( Header* node ) {
		var bucketIndex = getBucketIndex( node->Size );
		var bucketNode = freeBuckets[bucketIndex];

		if ( bucketNode == null ) {
			freeBuckets[bucketIndex] = node;

			node->NextFreeBySize = null;
			node->PreviousFreeBySize = null;
		}
		else if ( bucketNode->Size >= node->Size ) {
			// we are now the first item in the bucket
			freeBuckets[bucketIndex] = node;
			node->NextFreeBySize = bucketNode;
			node->PreviousFreeBySize = bucketNode->PreviousFreeBySize;

			bucketNode->PreviousFreeBySize = node;
		}
		else {
			// we need to sort outselves
			while ( bucketNode->NextFreeBySize != null && bucketNode->NextFreeBySize->Size < node->Size ) {
				// TODO I wonder if we can delay this step until the next alloc. Basically while searching for a fit, it would "drag" this with it. or maybe just dont sort buckets
				bucketNode = bucketNode->NextFreeBySize;
			}

			if ( bucketNode->NextFreeBySize == null ) {
				// we are the biggest one
				bucketNode->NextFreeBySize = node;
				node->PreviousFreeBySize = bucketNode;
				node->NextFreeBySize = null;
			}
			else {
				// bucketNode->NextFreeBySize has bigger or equal size
				// and bucketNode has smaller size
				node->PreviousFreeBySize = bucketNode;
				node->NextFreeBySize = bucketNode->NextFreeBySize;
				bucketNode->NextFreeBySize->PreviousFreeBySize = node;
				bucketNode->NextFreeBySize = node;
			}
		}
	}

	[DoesNotReturn]
	void throwMagic2 () {
		throw new InvalidOperationException( "Tried to reallocate unallocated memory" );
	}
	[DoesNotReturn]
	void throwImLazy () { // TODO implement this
		throw new NotImplementedException( "Out of memory, cant be bothered to restore state and return null" );
	}

	public Allocation Reallocate ( void* ptr, nuint newSize, out bool moved ) {
		var node = (Header*)((nuint)ptr - SizeOfHelper<Header>.Size);

#if DEBUG_ALLOCATORS
		if ( node->Magic != Header.MagicValue )
			throwMagic2();
#endif

		var oldSize = node->Size;

		removeNode( node, getBucketIndex( oldSize ) );

		// basically free this node and merge it with next (if possible)
		var next = Header.GetNext( node );
		if ( next->IsFree ) {
			removeNode( next, getBucketIndex( next->Size ) );
			Header.GetNext( next )->Previous = node;
			node->Size += next->Size + SizeOfHelper<Header>.Size;
		}

		bool tryFit () {
			// if it fits, I sits (this is also good for shrinks)
			if ( node->Size == newSize ) {
				insertNode( node );
				return true;
			}
			else if ( node->Size >= newSize + SizeOfHelper<Header>.Size ) {
				// split into 2
				split( node, newSize );
				insertNode( node );
				return true;
			}

			return false;
		}

		if ( tryFit() ) {
			moved = false;
			return new( ptr, newSize );
		}

		// otherwise, try to merge with previous and do the same check
		moved = true;
		void* newPtr;
		var length = (int)nuint.Min( oldSize, newSize );

		var previous = node->Previous;
		if ( previous != null && previous->IsFree ) {
			removeNode( previous, getBucketIndex( previous->Size ) );
			Header.GetNext( node )->Previous = previous;
			previous->Size += node->Size + SizeOfHelper<Header>.Size;

			node = previous;
		}
		
		if ( tryFit() ) {
			newPtr = Header.Data( node );
			node->IsFree = false;
			new Span<byte>( ptr, length ).CopyTo( new Span<byte>( newPtr, length ) );

			return new( newPtr, newSize );
		}

		// if that fails too, we basically already freed this block, so we can just allocate a new one and move the data there
		// the edge case is when we are out of memory, in which case we need to restore this block and return null

		newPtr = Allocate( newSize );
		if ( newPtr == null )
			throwImLazy();
		new Span<byte>( ptr, length ).CopyTo( new Span<byte>( newPtr, length ) );

		node->IsFree = true;
		insertNode( node );

		return new( newPtr, newSize );
	}

	[DoesNotReturn]
	void throwMagic () {
		throw new InvalidOperationException( "Tried to free unallocated memory" );
	}

	public void Free ( void* ptr ) {
		var node = (Header*)((nuint)ptr - SizeOfHelper<Header>.Size);
#if DEBUG_ALLOCATORS
		if ( node->Magic != Header.MagicValue )
			throwMagic();
#endif

		node->IsFree = true;
		var previous = node->Previous;
		var next = Header.GetNext( node );

		if ( next->IsFree ) { // no null check, there is a dummy taken block at the end
			removeNode( next, getBucketIndex( next->Size ) );
			Header.GetNext( next )->Previous = node;
			node->Size += next->Size + SizeOfHelper<Header>.Size;
		}

		if ( previous != null && previous->IsFree ) {
			removeNode( previous, getBucketIndex( previous->Size ) );
			Header.GetNext( node )->Previous = previous;
			previous->Size += node->Size + SizeOfHelper<Header>.Size;

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

		var head = @base;
		var tail = (Header*)((nuint)@base + totalSize - SizeOfHelper<Header>.Size);

		int headers = 0;
		while ( head != tail ) {
			headers++;
			sb.AppendLine( $"\t[{(head->IsFree ? "Free" : "Taken")} {formatSize(head->Size)} <+{formatSize(SizeOfHelper<Header>.Size)}>]" );
			head = Header.GetNext( head );
		}
		sb.AppendLine();
		sb.AppendLine( $"Total Metadata: {formatSize((headers + 1) * SizeOfHelper<Header>.Size)}" );
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

				sb.Append( $"{formatSize(head->Size)}" );
				head = head->NextFreeBySize;

				isfirst = false;
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	[StructLayout( LayoutKind.Sequential )]
	struct Header {
		// metadata about memory region ahead
		public bool IsFree;
		public nuint Size;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void* Data ( Header* @this ) {
			return (void*)((nuint)@this + SizeOfHelper<Header>.Size);
		}

		// sorted linked list, by size. only links to other nodes in the same bucket
		public Header* PreviousFreeBySize;
		public Header* NextFreeBySize;

		// metadata about physicaly adjacent regions
		public Header* Previous;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Header* GetNext ( Header* @this ) {
			return (Header*)((nuint)@this + SizeOfHelper<Header>.Size + @this->Size);
		}

#if DEBUG_ALLOCATORS
		public const long MagicValue = 0x0123456789ABCDEF0L;
		public long Magic;
#endif
	}
}
