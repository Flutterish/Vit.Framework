namespace Vit.Framework.Memory.Allocation;

public abstract class ManagedRegionAllocator<TAllocation, TRegion> where TRegion : Region<TRegion> where TAllocation : IRegionAllocation<TRegion> {
	public TAllocation Allocate () {
		if ( !freeRegions.TryPeek( out var region ) ) {
			region = CreateRegion( lastRegion );
			lastRegion = region;
			freeRegions.Push( region );
		}

		var allocation = Allocate( region );
		if ( !region.HasFreeSpace ) {
			freeRegions.Pop();
		}
		return allocation;
	}

	public void Free ( TAllocation allocation ) {
		var region = allocation.Region;
		if ( !allocation.Region.HasFreeSpace ) {
			freeRegions.Push( region );
		}

		Free( region, allocation );
	}

	protected abstract TRegion CreateRegion ( TRegion? last );
	protected abstract TAllocation Allocate ( TRegion region );
	protected abstract void Free ( TRegion region, TAllocation allocation );

	TRegion? lastRegion;
	Stack<TRegion> freeRegions = new();

	protected IEnumerable<TRegion> Regions {
		get {
			var region = lastRegion;
			while ( region != null ) {
				yield return region;
				region = region.Previous!;
			}
		}
	}
}

public interface IRegionAllocation<out TRegion> where TRegion : Region<TRegion> {
	TRegion Region { get; }
}

public abstract class Region<TSelf> where TSelf : Region<TSelf> {
	public readonly TSelf? Previous;

	protected Region ( TSelf? previous ) {
		Previous = previous;
	}

	public abstract bool HasFreeSpace { get; }
}