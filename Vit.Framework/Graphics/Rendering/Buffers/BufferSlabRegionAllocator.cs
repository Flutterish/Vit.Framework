using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// A simple allocator for buffers, which creates buffers of a predefined size and lends fixed-size portions of them.
/// </summary>
public class BufferSlabRegionAllocator<TBuffer> : DisposableObject where TBuffer : IBuffer {
	public delegate TBuffer BufferCreator ( IRenderer renderer, uint size );
	BufferCreator creator;

	public readonly uint RegionSize;
	public readonly uint SlabSize;
	/// <param name="regionSize">Amount of slabs per buffer.</param>
	/// <param name="slabSize">Amount of elements per slab.</param>
	/// <param name="creator">A function that creates a buffer with space for <c>size</c> elements.</param>
	public BufferSlabRegionAllocator ( uint regionSize, uint slabSize, BufferCreator creator ) {
		this.creator = creator;
		RegionSize = regionSize;
		SlabSize = slabSize;
	}

	Region? lastRegion;
	Stack<Region> freeRegions = new();
	public Allocation Allocate ( IRenderer renderer ) {
		if ( !freeRegions.TryPeek( out var region ) ) {
			region = new( creator( renderer, RegionSize * SlabSize ), this, lastRegion );
			lastRegion = region;
			freeRegions.Push( region );
		}

		var allocation = region.Allocate();
		if ( !region.HasFreeSlabs ) {
			freeRegions.Pop();
		}
		return allocation;
	}

	public void Free ( Allocation allocation ) {
		if ( !allocation.Region.HasFreeSlabs ) {
			freeRegions.Push( allocation.Region );
		}

		allocation.Region.Free( allocation.Offset );
	}

	public class Region {
		public readonly Region? Previous;
		public readonly BufferSlabRegionAllocator<TBuffer> Owner;
		public readonly TBuffer Buffer;

		Stack<uint> freeSlots;

		internal Region ( TBuffer buffer, BufferSlabRegionAllocator<TBuffer> owner, Region? previous ) {
			Buffer = buffer;
			Owner = owner;
			Previous = previous;

			freeSlots = new( (int)owner.RegionSize );
			for ( uint i = owner.RegionSize - 1; i != uint.MaxValue; i-- ) {
				freeSlots.Push( i * owner.SlabSize );
			}
		}

		public bool HasFreeSlabs => freeSlots.Any();

		internal Allocation Allocate () {
			return new() {
				Buffer = Buffer,
				Offset = freeSlots.Pop(),
				Size = Owner.SlabSize,
				Region = this
			};
		}

		internal void Free ( uint slot ) {
			freeSlots.Push( slot );
		}
	}

	public struct Allocation {
		public TBuffer Buffer;
		public uint Offset;
		public uint Size;

		public Region Region;
	}

	protected override void Dispose ( bool disposing ) {
		var region = lastRegion;
		while ( region != null ) {
			region.Buffer.Dispose();
			region = region.Previous;
		}
	}
}
