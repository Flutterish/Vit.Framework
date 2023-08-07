using System.Diagnostics;

namespace Vit.Framework.Collections;

/// <summary>
/// A stable-sorted doubly linked list.
/// </summary>
/// <typeparam name="TKey">The value to sort by.</typeparam>
/// <typeparam name="TValue">The stored value.</typeparam>
public sealed class SortedLinkedList<TKey, TValue> {
	IComparer<TKey> comparer;
	public SortedLinkedList ( IComparer<TKey>? comparer = null ) {
		this.comparer = comparer ?? Comparer<TKey>.Default;
	}

	public IEnumerable<TValue> Values {
		get {
			var node = First;
			while ( node != null ) {
				yield return node.Value;
				node = node.Next;
			}
		}
	}

	public int Count { get; private set; }
	public Node? First { get; private set; }
	public Node? Last { get; private set; }

	/// <summary>
	/// Adds a new node, starting the sorting process from the last node.
	/// </summary>
	public Node AddLast ( TKey key, TValue value ) {
		if ( Last == null )
			return First = Last = new Node( key, value, this );
		else
			return Last.AddAfter( key, value );
	}

	/// <summary>
	/// Adds a new node, starting the sorting process from the first node.
	/// </summary>
	public Node AddFirst ( TKey key, TValue value ) {
		if ( First == null )
			return First = Last = new Node( key, value, this );
		else
			return First.AddBefore( key, value );
	}

	public sealed class Node {
		public readonly TKey Key;
		public readonly TValue Value;
		public SortedLinkedList<TKey, TValue> List { get; private set; }

		internal Node ( TKey key, TValue value, SortedLinkedList<TKey, TValue> list ) {
			Key = key;
			Value = value;
			List = list;

			list.Count++;
		}

		public Node? Next { get; private set; }
		public Node? Previous { get; private set; }

		/// <summary>
		/// Adds a new node, starting the sorting process from this node. If keys are equal, the node will be inserted right after this one.
		/// </summary>
		public Node AddAfter ( TKey key, TValue value ) {
			Debug.Assert( List != null, "Attempted to modify a removed node" );

			var compare = List.comparer.Compare( Key, key );
			if ( compare == 0 ) {
				var node = new Node( key, value, List );
				node.Previous = this;

				if ( Next == null )
					return Next = List.Last = node;

				node.Next = Next;
				Next.Previous = node;
				Next = node;

				return node;
			}

			return add( this, key, value, compare );
		}

		/// <summary>
		/// Adds a new node, starting the sorting process from this node. If keys are equal, the node will be inserted right before this one.
		/// </summary>
		public Node AddBefore ( TKey key, TValue value ) {
			Debug.Assert( List != null, "Attempted to modify a removed node" );

			var compare = List.comparer.Compare( Key, key );
			if ( compare == 0 ) {
				var node = new Node( key, value, List );
				node.Next = this;

				if ( Previous == null )
					return Previous = List.First = node;

				node.Previous = Previous;
				Previous.Next = node;
				Previous = node;

				return node;
			}

			return add( this, key, value, compare );
		}

		static Node add ( Node current, TKey key, TValue value, int compare ) {
			var list = current.List;
			var comparer = list.comparer;
			var node = new Node( key, value, list );

			while ( true ) {
				if ( compare > 0 ) { // its before us
					if ( current.Previous == null ) {
						node.Next = current;
						return list.First = current.Previous = node;
					}

					compare = comparer.Compare( current.Previous.Key, key );
					if ( compare > 0 ) // even further before
						current = current.Previous;
					else { // equal or after the previous node
						node.Previous = current.Previous;
						node.Next = current;
						current.Previous.Next = node;
						current.Previous = node;

						return node;
					}
				}
				else { // its after us, the = 0 case is already covered by calling method or previous iteration
					if ( current.Next == null ) {
						node.Previous = current;
						return list.Last = current.Next = node;
					}

					compare = comparer.Compare( current.Next.Key, key );
					if ( compare < 0 ) // even further after
						current = current.Next;
					else { // equal or before the next node
						node.Previous = current;
						node.Next = current.Next;
						current.Next.Previous = node;
						current.Next = node;

						return node;
					}
				}
			}
		}

		/// <summary>
		/// Removes this node, linking its neighbours instead.
		/// </summary>
		public void Remove () {
			Debug.Assert( List != null, "Attempted to modify a removed node" );

			List.Count--;
			if ( Previous == null )
				List.First = Next;
			if ( Next == null )
				List.Last = Previous;

			if ( Previous != null ) {
				Previous.Next = Next;
			}
			if ( Next != null ) {
				Next.Previous = Previous;
			}

			List = null!;
		}

		public override string ToString () {
			return $"{Value} @ {Key}";
		}
	}
}