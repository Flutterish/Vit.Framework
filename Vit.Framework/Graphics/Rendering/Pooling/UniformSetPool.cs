using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.Rendering.Pooling;

/// <summary>
/// A simple allocator for <see cref="IUniformSet"/>s, which creates <see cref="IUniformSetPool"/>s of a predefined size and lends individual uniform sets.
/// </summary>
public class UniformSetPool : RegionAllocator<UniformSetPool.Allocation, UniformSetPool.Region>, IDisposable {
	public readonly uint PoolSize;
	public readonly uint Set;
	public readonly IShaderSet Shaders;
	public UniformSetPool ( IShaderSet shaders, uint set, uint poolSize ) {
		PoolSize = poolSize;
		Set = set;
		Shaders = shaders;
	}

	protected override Region CreateRegion ( Region? last ) {
		return new Region( last, Shaders.CreateUniformSetPool( Set, PoolSize ), PoolSize );
	}

	protected override Allocation Allocate ( Region region ) {
		region.FreeCount--;
		return new() {
			Region = region,
			UniformSet = region.Pool.Rent()
		};
	}

	protected override void Free ( Region region, Allocation allocation ) {
		region.FreeCount++;
		region.Pool.Free( allocation.UniformSet );
	}

	public class Region : Region<Region> {
		public uint FreeCount;
		public readonly IUniformSetPool Pool;
		public Region ( Region? previous, IUniformSetPool pool, uint count ) : base( previous ) {
			FreeCount = count;
			Pool = pool;
		}

		public override bool HasFreeSpace => FreeCount != 0;
	}

	public struct Allocation : IRegionAllocation<Region> {
		public Region Region { get; init; }
		public IUniformSet UniformSet;
	}

	public void Dispose () {
		foreach ( var i in Regions ) {
			i.Pool.Dispose();
		}
	}
}