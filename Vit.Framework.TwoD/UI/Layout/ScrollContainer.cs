using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Composite;
using Vit.Framework.TwoD.UI.Input.Events;

namespace Vit.Framework.TwoD.UI.Layout;

public class ScrollContainer : ScrollContainer<UIComponent> { }
public class ScrollContainer<T> : InternalContainer, IDraggable where T : UIComponent {
	public ScrollContainer () {
		IsMaskingActive = true;
	}

	public required LayoutDirection ScrollDirection;
	public RelativeSpacing<float> AllowedOverscroll;

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

	EmptyHoverableUIComponent dragReceptor = null!;
	public required T Content {
		get => (T)Children[1];
		init {
			AddInternalChild( dragReceptor = new() );
			AddInternalChild( value );
		}
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
		if ( !ScrollDirection.HasFlag( LayoutDirection.Vertical ) )
			scrollValue.Y = 0;
		if ( !ScrollDirection.HasFlag( LayoutDirection.Horizontal ) )
			scrollValue.X = 0;

		var scrollBounds = getScrollBounds();

		scrollValue.X = float.Max( float.Min( scrollValue.X, scrollBounds.MaxX ), scrollBounds.MinX );
		scrollValue.Y = float.Max( float.Min( scrollValue.Y, scrollBounds.MaxY ), scrollBounds.MinY );
		Content.Position = new Point2<float>() {
			X = contentAnchor.X.GetValue( Width ) - contentOrigin.X.GetValue( Content.Width ),
			Y = contentAnchor.Y.GetValue( Height ) - contentOrigin.Y.GetValue( Content.Height )
		} + scrollValue;

		dragReceptor.Size = Size;
	}

	AxisAlignedBox2<float> getScrollBounds () {
		var overflowX = float.Max( Content.Width - Width, 0 );
		var overflowY = float.Max( Content.Height - Height, 0 );

		return new() {
			MinX = -overflowX - AllowedOverscroll.Left.GetValue( float.Min( Width, Content.Width ) ) ,
			MinY = -AllowedOverscroll.Top.GetValue( float.Min( Height, Content.Height ) ),
			MaxX = AllowedOverscroll.Right.GetValue( float.Min( Width, Content.Width ) ),
			MaxY = overflowY + AllowedOverscroll.Bottom.GetValue( float.Min( Height, Content.Height ) )
		};
	}

	bool usingMiddleDrag;
	Vector2<float> middleDragStrength;
	public override void Update () {
		if ( usingMiddleDrag ) {
			ScrollValue -= middleDragStrength * (float)(Clock.ElapsedTime * 0.01d.PerMilli());
		}
		base.Update();
	}

	public bool OnPressed ( PressedEvent @event ) {
		return true;
	}

	public bool OnDragStarted ( DragStartedEvent @event ) { // TODO after reaching scroll limit, fall through again
		if ( @event.Button == Framework.Input.CursorButton.Middle )
			usingMiddleDrag = true;
		return true;
	}

	public bool OnDragged ( DraggedEvent @event ) { // TODO middle button scroll visual
		if ( @event.Button == Framework.Input.CursorButton.Right ) {
			var delta = ScreenSpaceDeltaToLocalSpace( @event.DeltaPosition );
			var scrollBounds = getScrollBounds();
			ScrollValue -= new Vector2<float>() {
				X = delta.X / Width * scrollBounds.Width,
				Y = delta.Y / Height * scrollBounds.Height
			};
		}
		else if ( @event.Button == Framework.Input.CursorButton.Left ) {
			ScrollValue += ScreenSpaceDeltaToLocalSpace( @event.DeltaPosition );
		}
		else if ( @event.Button == Framework.Input.CursorButton.Middle ) {
			middleDragStrength = ScreenSpaceDeltaToLocalSpace( @event.EventPosition - @event.EventStartPosition );
		}

		return true;
	}

	public bool OnDragEnded ( DragEndedEvent @event ) {
		if ( @event.Button == Framework.Input.CursorButton.Middle )
			usingMiddleDrag = false;
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return false;
	}
}
