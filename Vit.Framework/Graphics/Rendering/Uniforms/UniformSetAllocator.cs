using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering.Uniforms;

/// <summary>
/// A simple allocator for <see cref="IUniformSet"/>s, which creates <see cref="IUniformSetPool"/>s of a predefined size and lends individual uniform sets.
/// </summary>
public class UniformSetAllocator : DisposableObject {
	public readonly uint PoolSize;
	public readonly UniformSetInfo Type;
	public UniformSetAllocator ( UniformSetInfo type, uint poolSize ) {
		PoolSize = poolSize;
		Type = type;
	}

	Region? lastRegion; // TODO instead of individual uploads to the buffer, we could batch them
	Stack<Region> freeRegions = new();
	public Allocation Allocate ( IRenderer renderer ) {
		if ( !freeRegions.TryPeek( out var region ) ) {
			region = new( renderer.CreateUniformSetPool( PoolSize, Type ), this, lastRegion );
			lastRegion = region;
			freeRegions.Push( region );
		}

		var allocation = region.Allocate();
		if ( region.FreeCount == 0 ) {
			freeRegions.Pop();
		}
		return allocation;
	}

	public void Free ( Allocation allocation ) {
		if ( allocation.Region.FreeCount == 0 ) {
			freeRegions.Push( allocation.Region );
		}

		allocation.Region.Free( allocation.UniformSet );
	}

	public class Region {
		public readonly Region? Previous;
		public readonly UniformSetAllocator Owner;
		public readonly IUniformSetPool Pool;
		public uint FreeCount;

		internal Region ( IUniformSetPool pool, UniformSetAllocator owner, Region? previous ) {
			FreeCount = owner.PoolSize;
			Owner = owner;
			Previous = previous;
			Pool = pool;
		}

		internal Allocation Allocate () {
			FreeCount--;
			return new() {
				UniformSet = Pool.Rent(),
				Region = this
			};
		}

		internal void Free ( IUniformSet set ) {
			FreeCount++;
			Pool.Free( set );
		}
	}

	public struct Allocation {
		public IUniformSet UniformSet;
		public Region Region;
	}

	protected override void Dispose ( bool disposing ) {
		var region = lastRegion;
		while ( region != null ) {
			region.Pool.Dispose();
			region = region.Previous;
		}
	}
}
