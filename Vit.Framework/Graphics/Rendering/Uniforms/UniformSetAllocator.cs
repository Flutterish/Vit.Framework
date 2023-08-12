using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Memory.Allocation;

namespace Vit.Framework.Graphics.Rendering.Uniforms;

/// <summary>
/// A simple allocator for <see cref="IUniformSet"/>s, which creates <see cref="IUniformSetPool"/>s of a predefined size and lends individual uniform sets.
/// </summary>
public class UniformSetAllocator : ManagedRegionAllocator<UniformSetAllocator.Allocation, UniformSetAllocator.Region>, IDisposable {
	public readonly uint PoolSize;
	public readonly UniformSetInfo Type;
	public readonly IRenderer Renderer;
	public UniformSetAllocator ( UniformSetInfo type, IRenderer renderer, uint poolSize ) {
		PoolSize = poolSize;
		Type = type;
		Renderer = renderer;
	}

	protected override Region CreateRegion ( Region? last ) {
		return new Region( last, Renderer.CreateUniformSetPool( PoolSize, Type ), PoolSize );
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