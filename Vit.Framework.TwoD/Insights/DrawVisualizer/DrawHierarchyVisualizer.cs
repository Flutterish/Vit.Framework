using System.Diagnostics.CodeAnalysis;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Input;
using Vit.Framework.TwoD.UI.Input.Events;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

public class DrawHierarchyVisualizer : ScrollContainer<LayoutContainer> {
	[SetsRequiredMembers]
	public DrawHierarchyVisualizer () {
		ScrollDirection = LayoutDirection.Vertical;
		AllowedOverscroll = new() { Bottom = 1f.Relative() - 40 - 30 };
		ContentSize = new( 1f.Relative(), 0 );
		ContentAnchor = Anchor.TopLeft;
		ContentOrigin = Anchor.TopLeft;
		Content = new() {
			AutoSizeDirection = LayoutDirection.Vertical,
			Padding = new( 10 )
		};
	}

	Node? topNode;
	public Action<IViewableInDrawVisualiser?>? Selected;
	public void View ( IViewableInDrawVisualiser? target ) {
		if ( target == null ) {
			if ( topNode != null ) {
				Content.NoUnloadRemoveChild( topNode );
				freeNode( topNode );
			}
		}
		else {
			if ( topNode == null ) {
				Content.AddChild( topNode = getNode(), new() {
					Size = new( 1f.Relative(), 1f.Relative() ),
					Anchor = Anchor.TopLeft,
					Origin = Anchor.TopLeft
				} );
			}
			topNode.View( target );
		}

		Selected?.Invoke( target );
	}

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();
		foreach ( var i in nodePool ) {
			i.DisposeDrawNodes();
		}
	}

	Stack<Node> nodePool = new();
	Node getNode () {
		if ( !nodePool.TryPop( out var node ) ) {
			node = new( this );
			node.Selected = t => Selected?.Invoke(t);
			return node;
		}
		return node;
	}
	void freeNode ( Node node ) {
		nodePool.Push( node );
	}

	class Node : FlowContainer {
		DrawHierarchyVisualizer source;
		BasicButton button;
		FlowContainer<Node> children;
		public Node ( DrawHierarchyVisualizer source ) {
			this.source = source;
			FlowDirection = FlowDirection.Down;
			AddChild( button = new HoverableBasicButton() {
				Clicked = () => {
					IsOpened = !IsOpened;
					Selected?.Invoke( target );
				},
				Hovered = () => Selected?.Invoke( target ),
				TextAnchor = Anchor.Centre,
				TextOrigin = Anchor.Centre
			}, new() {
				Size = new( 1f.Relative(), 40 )
			} );
			AddChild( children = new() {
				FlowDirection = FlowDirection.Down,
				Padding = new() { Left = 20 }
			}, new() {
				Size = new( 1f.Relative(), 0 )
			} );
		}

		IViewableInDrawVisualiser target = null!;
		public void View ( IViewableInDrawVisualiser target ) {
			this.target = target;
			IsOpened = false;
		}

		bool isOpened;
		Dictionary<IViewableInDrawVisualiser, Node> childBySource = new();
		bool IsOpened {
			get => isOpened;
			set {
				if ( !value.TrySet( ref isOpened ) )
					return;

				if ( !value ) {
					foreach ( var i in children.Children ) {
						source.freeNode( i );
					}
					children.NoUnloadClearChildren();
					childBySource.Clear();
				}
			}
		}

		bool stillAlive;
		public override void Update () {
			button.RawText = target.Name;
			if ( !isOpened ) {
				base.Update();
				return;
			}

			foreach ( var i in children.Children ) {
				i.stillAlive = false;
			}
			foreach ( var i in target.Children ) {
				if ( childBySource.TryGetValue( i, out var node ) )
					node.stillAlive = true;
			}
			for ( int i = children.Children.Count - 1; i >= 0; i-- ) {
				var child = children.Children[i];
				if ( !child.stillAlive ) {
					children.NoUnloadRemoveChildAt( i );
					source.freeNode( child );
					childBySource.Remove( child.target );
				}
			}

			foreach ( var target in target.Children ) {
				if ( target is DrawVisualizer )
					continue;

				if ( childBySource.TryGetValue( target, out var node ) )
					continue;

				node = source.getNode();
				node.View( target );
				childBySource.Add( target, node );
				children.AddChild( node, new() {
					Size = new( 1f.Relative(), 0 ),
					Margins = new() { Vertical = 10 }
				} );
			}

			int index = 0;
			foreach ( var target in target.Children ) {
				if ( target is DrawVisualizer )
					continue;

				var node = childBySource[target];
				if ( node.Depth != index ) {
					children.NoUnloadRemoveChild( node );
					children.InsertChild( index, node, new() {
						Size = new( 1f.Relative(), 0 ),
						Margins = new() { Vertical = 10 }
					} );
				}

				index++;
			}

			base.Update();
		}

		public Action<IViewableInDrawVisualiser?>? Selected;
		class HoverableBasicButton : BasicButton, IFocusable {
			protected override void OnStateChanged ( ButtonState state ) {
				if ( state is ButtonState.Hovered )
					Hovered?.Invoke();
				base.OnStateChanged( state );
			}

			public Action? Hovered;

			bool IFocusable.OnFocused ( FocusGainedEvent @event ) {
				if ( base.OnFocused( @event ) ) {
					Hovered?.Invoke();
					return true;
				}
				return false;
			}
		}
	}
}
