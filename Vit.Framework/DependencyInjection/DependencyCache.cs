﻿namespace Vit.Framework.DependencyInjection;

public class DependencyCache : IDependencyCache {
	public readonly IReadonlyDependencyCache? Parent;
	public DependencyCache ( IReadonlyDependencyCache? parent = null ) {
		Parent = parent;
	}

	Dictionary<DependencyIdentifier, object?> store = new();
	Dictionary<DependencyIdentifier, object?> cache = new(); // TODO maybe only store in cache if the dependency was requested multiple times?
	public void Cache ( object? value, DependencyIdentifier identifier ) {
		store.Add( identifier, value );
		cache[identifier] = value;
	}

	public object? Resolve ( DependencyIdentifier identifier ) {
		if ( !cache.TryGetValue( identifier, out var value ) ) {
			if ( Parent == null )
				throw new Exception( $"Could not resolve dependency: {identifier}" );
			cache[identifier] = value = Parent.Resolve( identifier );
		}

		return value;
	}
}

public interface IDependencyCache : IReadonlyDependencyCache {
	void Cache ( object? value, DependencyIdentifier identifier );
}

public interface IReadonlyDependencyCache {
	object? Resolve ( DependencyIdentifier identifier );
}

public static class DependencyCacheExtensions {
	public static T Resolve<T> ( this IReadonlyDependencyCache cache )
		=> cache.Resolve<T>( new DependencyIdentifier() { Type = typeof( T ) } );
	public static T Resolve<T> ( this IReadonlyDependencyCache cache, string name )
		=> cache.Resolve<T>( new DependencyIdentifier() { Type = typeof( T ), Name = name } );
	public static T Resolve<T> ( this IReadonlyDependencyCache cache, DependencyIdentifier identifier )
		=> (T)cache.Resolve( identifier )!;

	public static void Cache<T> ( this IDependencyCache cache, T value )
		=> cache.Cache( value, new DependencyIdentifier() { Type = typeof( T ) } );
	public static void Cache<T> ( this IDependencyCache cache, T value, string name )
		=> cache.Cache( value, new DependencyIdentifier() { Type = typeof( T ), Name = name } );
}