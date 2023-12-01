using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.Rendering.Pooling;

public class SingleUseUniformSetPool : IDisposable {
	public readonly uint PoolSize;
	public readonly uint Set;
	public readonly IShaderSet Shaders;
	public SingleUseUniformSetPool ( IShaderSet shaders, uint set, uint poolSize ) {
		PoolSize = poolSize;
		Set = set;
		Shaders = shaders;
	}

	List<IUniformSetPool> pools = new();
	Stack<IUniformSet> remainingSets = new();
	Stack<IUniformSet> rentedSets = new();

	public void EndFrame () {
		while ( rentedSets.TryPop( out var set ) ) {
			remainingSets.Push( set );
		}
	}

	public IUniformSet Allocate () {
		if ( remainingSets.TryPop( out var set ) ) {
			rentedSets.Push( set );
			return set;
		}

		var length = PoolSize << pools.Count;
		var pool = Shaders.CreateUniformSetPool( Set, length );
		pools.Add( pool );
		for ( uint i = 0; i < length; i++ ) {
			remainingSets.Push( pool.Rent() );
		}

		set = remainingSets.Pop();
		rentedSets.Push( set );
		return set;
	}

	public void Dispose () {
		foreach ( var i in pools ) {
			i.Dispose();
		}
		pools.Clear();
		remainingSets.Clear();
		rentedSets.Clear();
	}
}
