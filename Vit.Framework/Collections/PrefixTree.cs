﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Vit.Framework.Collections;

public abstract class PrefixTree<TKey, TChunk, TValue> where TChunk : notnull {
	bool hasValue;
	TValue value = default!;
	Dictionary<TChunk, PrefixTree<TKey, TChunk, TValue>>? children;

	public PrefixTree<TKey, TChunk, TValue>? TryGetNode ( TChunk chunk ) {
		return children?.GetValueOrDefault( chunk );
	}

	public PrefixTree<TKey, TChunk, TValue> GetOrCreateNode ( TKey key ) {
		var chunks = GetChunks( key );
		var node = this;
		foreach ( var i in chunks ) {
			node.children ??= new();
			if ( !node.children.TryGetValue( i, out var next ) )
				node.children.Add( i, next = CreateNode() );

			node = next;
		}

		return node;
	}

	public bool HasValue => hasValue;
	public TValue Value => value;

	public void SetValue ( TValue value ) {
		hasValue = true;
		this.value = value;
	}

	public IEnumerable<KeyValuePair<TChunk, PrefixTree<TKey, TChunk, TValue>>> Children
		=> children ?? Enumerable.Empty<KeyValuePair<TChunk, PrefixTree<TKey, TChunk, TValue>>>();

	protected abstract PrefixTree<TKey, TChunk, TValue> CreateNode ();
	protected abstract IEnumerable<TChunk> GetChunks ( TKey key );

	public void Add ( TKey key, TValue value ) {
		var node = GetOrCreateNode( key );

		node.hasValue = true;
		node.value = value;
	}

	public bool Remove ( TKey key, [NotNullWhen( true )] out TValue? value ) {
		var node = this;
		foreach ( var i in GetChunks( key ) ) {
			if ( node.children == null || !node.children.TryGetValue( i, out node ) ) {
				value = default;
				return false;
			}
		}

		value = node.value!;
		if ( !node.hasValue ) {
			return false;
		}
		node.hasValue = false;
		return true;
	}

	public void Clear () {
		children?.Clear();
		hasValue = false;
	}

	public bool TryGet ( TKey key, [NotNullWhen( true )] out TValue? value ) {
		var node = this;
		foreach ( var i in GetChunks( key ) ) {
			if ( node.children == null || !node.children.TryGetValue( i, out node ) ) {
				value = default;
				return false;
			}
		}

		value = node.value!;
		return node.hasValue;
	}
}

public class StringPrefixTree<TValue> : PrefixTree<string, char, TValue> {
	protected override IEnumerable<char> GetChunks ( string key ) {
		return key.AsEnumerable();
	}

	protected override PrefixTree<string, char, TValue> CreateNode () {
		return new StringPrefixTree<TValue>();
	}
}

public class EnumerablePrefixTree<TKey, TValue> : PrefixTree<IEnumerable<TKey>, TKey, TValue> where TKey : notnull {
	protected override PrefixTree<IEnumerable<TKey>, TKey, TValue> CreateNode () {
		return new EnumerablePrefixTree<TKey, TValue>();
	}

	protected override IEnumerable<TKey> GetChunks ( IEnumerable<TKey> key ) {
		return key;
	}
}

[DebuggerTypeProxy( typeof( ByteCharPrefixTree<>.DebugDisplay ) )]
public class ByteCharPrefixTree<TValue> {
	bool hasValue;
	TValue value = default!;
	ByteCharPrefixTree<TValue>?[]? children;

	public void Add ( ReadOnlySpan<byte> key, TValue value ) {
		var self = this;

		ref var node = ref self;
		foreach ( var i in key ) {
			if ( node.children == null )
				node.children = new ByteCharPrefixTree<TValue>[256];
			node = ref node.children[i];
			node ??= new();
		}

		node.hasValue = true;
		node.value = value;
	}

	public bool ContainsKey ( ReadOnlySpan<byte> key ) {
		var node = this;
		foreach ( var i in key ) {
			if ( node.children == null )
				return false;
			node = node.children[i];
			if ( node == null )
				return false;
		}

		return node.hasValue;
	}

	public bool TryGetValue ( ReadOnlySpan<char> key, [NotNullWhen( true )] out TValue? value ) {
		return TryGetValue( MemoryMarshal.AsBytes( key ), out value );
	}

	public bool TryGetValue ( ReadOnlySpan<byte> key, [NotNullWhen( true )] out TValue? value ) {
			var node = this;
		foreach ( var i in key ) {
			if ( node.children == null ) {
				value = default;
				return false;
			}

			node = node.children[i];
			if ( node == null ) {
				value = default;
				return false;
			}
		}

		value = node.value;
		return node.hasValue;
	}

	public TValue GetValue ( ReadOnlySpan<byte> key ) {
		var node = this;
		foreach ( var i in key ) {
			node = node.children![i]!;
		}

		return node.value;
	}

	public TValue this[ReadOnlySpan<byte> key] {
		get => GetValue( key );
		set => Add( key, value );
	}

	public TValue this[ReadOnlySpan<char> key] {
		get => GetValue( MemoryMarshal.AsBytes( key ) );
		set => Add( MemoryMarshal.AsBytes( key ), value );
	}

	public override string ToString () {
		return $"{(hasValue ? value : "")}{(children == null ? "" : "+")}";
	}

	internal class DebugDisplay {
		public bool HasValue;
		public TValue Value;
		public Dictionary<string, ByteCharPrefixTree<TValue>?>? Children;

		public DebugDisplay ( ByteCharPrefixTree<TValue> tree ) {
			HasValue = tree.hasValue;
			Value = tree.value;
			if ( tree.children == null )
				return;

			Children = new( 256 );
			for ( int i = 0; i < 256; i++ ) {
				Children.Add( $"0x{i:X2}", tree.children[i] );
			}
		}
	}
}