using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.Rendering.Pooling;

/// <summary>
/// A simple allocator for buffers, which creates buffers of a predefined size and lends fixed-size portions of them.
/// </summary>
public class BufferSectionPool<TBuffer> : RegionAllocator<BufferSectionPool<TBuffer>.Allocation, BufferSectionPool<TBuffer>.Region>, IDisposable where TBuffer : IBuffer {
	public delegate TBuffer BufferCreator ( IRenderer renderer, uint size );
	public readonly IRenderer Renderer;
	BufferCreator creator;

	public readonly uint RegionSize;
	public readonly uint SlabSize;
	/// <param name="regionSize">Amount of slabs per buffer.</param>
	/// <param name="slabSize">Amount of elements per slab.</param>
	/// <param name="creator">A function that creates a buffer with space for <c>size</c> elements.</param>
	public BufferSectionPool ( uint regionSize, uint slabSize, IRenderer renderer, BufferCreator creator ) {
		this.creator = creator;
		RegionSize = regionSize;
		SlabSize = slabSize;
		Renderer = renderer;
	}

	// TODO instead of individual uploads to the buffer, we could batch them
	protected override Region CreateRegion ( Region? last ) {
		return new Region( last, creator( Renderer, RegionSize * SlabSize ), this );
	}

	protected override Allocation Allocate ( Region region ) {
		return new() {
			Region = region,
			Buffer = region.Buffer,
			Size = SlabSize,
			Offset = region.Allocate()
		};
	}

	protected override void Free ( Region region, Allocation allocation ) {
		region.Free( allocation.Offset );
	}

	public class Region : Region<Region> {
		public readonly TBuffer Buffer;

		Stack<uint> freeSlots;

		internal Region ( Region? previous, TBuffer buffer, BufferSectionPool<TBuffer> owner ) : base( previous ) {
			Buffer = buffer;

			freeSlots = new( (int)owner.RegionSize );
			for ( uint i = owner.RegionSize - 1; i != uint.MaxValue; i-- ) {
				freeSlots.Push( i * owner.SlabSize );
			}
		}

		public override bool HasFreeSpace => freeSlots.Any();

		internal uint Allocate () {
			return freeSlots.Pop();
		}

		internal void Free ( uint slot ) {
			freeSlots.Push( slot );
		}
	}

	public struct Allocation : IRegionAllocation<Region> {
		public TBuffer Buffer;
		public uint Offset;
		public uint Size;

		public Region Region { get; init; }
	}

	public void Dispose () {
		foreach ( var i in Regions ) {
			i.Buffer.Dispose();
		}
	}
}
