using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input.Events;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI;

public class DraggableContainer : DraggableContainer<UIComponent> { }
public class DraggableContainer<T> : Flexbox where T : UIComponent {
	Header header;
	public DraggableContainer () {
		FlowDirection = FlowDirection.Down;
		
		AddChild( header = new() {
			Tint = FrameworkUIScheme.Element,
			Dragged = delta => Dragged?.Invoke( delta )
		}, new() {
			Size = new( 1f.Relative(), 80 )
		} );
	}

	public required T Content {
		get => (T)Children[1];
		init {
			AddChild( value, new() {
				Size = new( 1f.Relative(), 0 ),
				Grow = 1
			} );
		}
	}

	public Action<Vector2<float>>? Dragged;

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
