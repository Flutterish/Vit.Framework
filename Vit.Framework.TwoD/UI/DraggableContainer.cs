using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input.Events;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI;

public class DraggableContainer : Flexbox { // TODO do this better
	Header header;
	public readonly LayoutContainer Content;
	public DraggableContainer () {
		FlowDirection = FlowDirection.Down;
		
		AddChild( header = new() {
			Tint = FrameworkUIScheme.Element,
			Dragged = delta => {
				Position += Parent!.ScreenSpaceDeltaToLocalSpace( delta );
			}
		}, new() {
			Size = new( 1f.Relative(), 80 )
		} );
		AddChild( Content = new() {
			LayoutChildren = new (UIComponent, LayoutParams)[] {
				(new Box { Tint = FrameworkUIScheme.Background }, new LayoutParams() {
					Size = new( 1f.Relative(), 1f.Relative() )
				})
			}
		}, new() {
			Size = new( 1f.Relative(), 20 ),
			Grow = 1
		} );
	}

	protected class Header : Box, IDraggable {
		public bool OnPressed ( PressedEvent @event ) {
			return @event.Button == Framework.Input.CursorButton.Left;
		}

		public bool OnDragStarted ( DragStartedEvent @event ) {
			return true;
		}

		public bool OnDragged ( DraggedEvent @event ) {
			Dragged?.Invoke( @event.DeltaPosition );
			return true;
		}

		public bool OnDragEnded ( DragEndedEvent @event ) {
			return true;
		}

		public Action<Vector2<float>>? Dragged;
	}
}
