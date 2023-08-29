using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Vit.Framework.Text.Fonts;

[DebuggerTypeProxy( typeof( CodepointPrefixTree<>.DebugDisplay ) )]
public class CodepointPrefixTree<TValue> {
	bool hasValue;
	TValue value = default!;
	Dictionary<uint, CodepointPrefixTree<TValue>>? children;

	public void Add ( UnicodeExtendedGraphemeCluster key, TValue value ) {
		var node = this;
		foreach ( var i in key ) {
			if ( node.children == null )
				node.children = new( 256 );
			if ( !node.children.TryGetValue( i, out var next ) )
				node.children.Add( i, next = new() );
			node = next;
		}

		node.hasValue = true;
		node.value = value;
	}

	public bool ContainsKey ( UnicodeExtendedGraphemeCluster key ) {
		var node = this;
		foreach ( var i in key ) {
			if ( node.children == null )
				return false;
			if ( !node.children.TryGetValue( i, out node ) )
				return false;
		}

		return node.hasValue;
	}

	public bool TryGetValue ( UnicodeExtendedGraphemeCluster key, [NotNullWhen( true )] out TValue? value ) {
		var node = this;
		foreach ( var i in key ) {
			if ( node.children == null ) {
				value = default;
				return false;
			}

			if ( !node.children.TryGetValue( i, out node ) ) {
				value = default;
				return false;
			}
		}

		value = node.value;
		return node.hasValue;
	}

	public TValue GetValue ( UnicodeExtendedGraphemeCluster key ) {
		var node = this;
		foreach ( var i in key ) {
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

			if ( tree.children == null )
				return;

			Children = new();
			foreach ( var (i, node) in tree.children ) {
				var rune = new Rune(i);
				var asText = Rune.IsControl( rune ) ? "" : $" ({rune})";
				Children.Add( $"U+{i:X8}{asText}", node );
			}
		}
	}
}