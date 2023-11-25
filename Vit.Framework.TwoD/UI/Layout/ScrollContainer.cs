using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Input.Events;

namespace Vit.Framework.TwoD.UI.Layout;

public class ScrollContainer : ScrollContainer<UIComponent> { }
public class ScrollContainer<T> : CompositeUIComponent<T>, IDraggable where T : UIComponent {
	public ScrollContainer () {
		IsMaskingActive = true;
	}

	RelativeAxes2<float> contentOrigin;
	public RelativeAxes2<float> ContentOrigin {
		get => contentOrigin;
		set {
			if ( value.TrySet( ref contentOrigin ) )
				InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	RelativeAxes2<float> contentAnchor;
	public RelativeAxes2<float> ContentAnchor {
		get => contentAnchor;
		set {
			if ( value.TrySet( ref contentAnchor ) )
				InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	RelativeSize2<float> contentSize;
	public RelativeSize2<float> ContentSize {
		get => contentSize;
		set {
			if ( value.TrySet( ref contentSize ) )
				InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	public required T Content {
		get => InternalChild;
		init => InternalChild = value;
	}

	Vector2<float> scrollValue;
	public Vector2<float> ScrollValue {
		get => scrollValue;
		set {
			if ( value.TrySet( ref scrollValue ) )
				InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	public override void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations ) {
		if ( invalidations.HasFlag( LayoutInvalidations.RequiredSize | LayoutInvalidations.Self ) )
			InvalidateLayout( LayoutInvalidations.Children | LayoutInvalidations.RequiredSize | LayoutInvalidations.Self );
		else
			InvalidateLayout( LayoutInvalidations.Children );
	}

	protected override void PerformSelfLayout () {
		Content.Size = contentSize.GetSize( Size ).Contain( Content.RequiredSize );
		Content.Position = new Point2<float>() {
			X = contentAnchor.X.GetValue( Width ) - contentOrigin.X.GetValue( Content.Width ),
			Y = contentAnchor.Y.GetValue( Height ) - contentOrigin.Y.GetValue( Content.Height )
		} + scrollValue;
	}

	public bool OnPressed ( PressedEvent @event ) {
		return true;
	}

	public bool OnDragStarted ( DragStartedEvent @event ) {
		return true;
	}

	public bool OnDragged ( DraggedEvent @event ) { // TODO middle button scroll
		if ( @event.Button == Framework.Input.CursorButton.Right ) {
			ScrollValue -= ScreenSpaceDeltaToLocalSpace( @event.DeltaPosition );
		}
		else if ( @event.Button == Framework.Input.CursorButton.Left ) {
			ScrollValue += ScreenSpaceDeltaToLocalSpace( @event.DeltaPosition );
		}

		return true;
	}

	public bool OnDragEnded ( DragEndedEvent @event ) {
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return true;
	}
}
