using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Vit.Framework.Text.Fonts;

[DebuggerTypeProxy( typeof( CodepointPrefixTree<>.DebugDisplay ) )]
public class CodepointPrefixTree<TValue> { // TODO optimise storage for the leading zeros
	bool hasValue;
	TValue value = default!;
	CodepointPrefixTree<TValue>?[]? children;

	public void Add ( UnicodeExtendedGraphemeCluster key, TValue value ) {
		var self = this;

		ref var node = ref self;
		foreach ( var i in key.Bytes ) {
			if ( node.children == null )
				node.children = new CodepointPrefixTree<TValue>?[256];
			node = ref node.children[i];
			node ??= new();
		}

		node.hasValue = true;
		node.value = value;
	}

	public bool ContainsKey ( UnicodeExtendedGraphemeCluster key ) {
		var node = this;
		foreach ( var i in key.Bytes ) {
			if ( node.children == null )
				return false;
			node = node.children[i];
			if ( node == null )
				return false;
		}

		return node.hasValue;
	}

	public bool TryGetValue ( UnicodeExtendedGraphemeCluster key, [NotNullWhen( true )] out TValue? value ) {
		var node = this;
		foreach ( var i in key.Bytes ) {
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

	public TValue GetValue ( UnicodeExtendedGraphemeCluster key ) {
		var node = this;
		foreach ( var i in key.Bytes ) {
			node = node.children![i]!;
		}

		return node.value;
	}

	public TValue this[UnicodeExtendedGraphemeCluster key] => GetValue( key );

	public override string ToString () {
		return $"{(hasValue ? value : "")}{(children == null ? "" : "+")}";
	}

	internal class DebugDisplay {
		public bool HasValue;
		public TValue Value;
		public SortedDictionary<string, CodepointPrefixTree<TValue>?>? Children;

		public DebugDisplay ( CodepointPrefixTree<TValue> tree ) {
			HasValue = tree.hasValue;
			Value = tree.value;

			Children = new();
			var codepointBytes = new int[4];
			void visit ( CodepointPrefixTree<TValue> tree, int index, Action<CodepointPrefixTree<TValue>> action ) {
				if ( tree.children == null )
					return;

				for ( int i = 0; i < 256; i++ ) {
					var child = tree.children[i];
					if ( child == null )
						continue;

					codepointBytes[index] = i;
					if ( index == 3 ) {
						action( child );
					}
					else {
						visit( child, index + 1, action );
					}
				}
			}

			visit( tree, 0, child => {
				var codepoint = ((uint)codepointBytes[3] << 24) | ((uint)codepointBytes[2] << 16) | ((uint)codepointBytes[1] << 8) | ((uint)codepointBytes[0] << 0);
				Children.Add( $"U+{codepoint:X8}", child );
			} );
		}
	}
}