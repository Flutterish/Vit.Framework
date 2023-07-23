using System.Diagnostics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public abstract class LayoutContainer<T, TParam> : CompositeDrawable<T>, ILayoutContainer<T, TParam, T>, ILayoutElement where T : ILayoutElement where TParam : struct {
	Size2<float> size;
	public Size2<float> ContentSize => new( size.Width - padding.Horizontal, size.Height - padding.Vertical );
	public Size2<float> Size {
		get => size;
		set {
			if ( size == value )
				return;

			InvalidateLayout();
			size = value;
		}
	}

	public override bool ReceivesPositionalInputAt ( Point2<float> point ) {
		point = ScreenSpaceToLocalSpace( point );

		return point.X >= 0 && point.X <= Size.Width
			&& point.Y >= 0 && point.Y <= Size.Height;
	}

	Size2<float>? requiredSize;
	public Size2<float> RequiredSize => requiredSize ??= PerformAbsoluteLayout();

	void autoSizeSelf ( Size2<float> size ) {
		requiredSize = size;
		this.size = size;
		isLayoutInvalidated = true;
	}

	Spacing<float> padding;
	public Spacing<float> Padding {
		get => padding;
		set {
			padding = value;
			InvalidateLayout();
		}
	}

	LayoutDirection autoSizeDirection;
	/// <summary>
	/// Directions in which this layout container should calculate its size based on children.
	/// </summary>
	public LayoutDirection AutoSizeDirection {
		get => autoSizeDirection;
		set {
			if ( autoSizeDirection == value )
				return;

			autoSizeDirection = value;
			InvalidateLayout();
		}
	}

	bool isParentLayoutContainer;
	protected override void OnParentChanged ( ICompositeDrawable<IDrawable>? from, ICompositeDrawable<IDrawable>? to ) {
		base.OnParentChanged( from, to );
		isParentLayoutContainer = to is ILayoutContainer;
	}

	public void UpdateLayoutParameters ( T child, TParam param ) {
		@params[child] = param;
		InvalidateLayout();
	}

	public TParam GetLayoutParameters ( T child ) {
		return @params[child];
	}

	Dictionary<T, TParam> @params = new();
	public virtual IEnumerable<(T child, TParam @param)> LayoutChildren {
		get {
			foreach ( var i in Children ) {
				yield return (i, @params[i]);
			}
		}
		set {
			ClearChildren();
			foreach ( var i in value ) {
				AddChild( i.child, i.param );
			}
		}
	}

	public void AddChild ( T child, TParam param ) {
		AddInternalChild( child );
		@params.Add( child, param );
		InvalidateLayout();
	}

	public bool RemoveChild ( T child ) {
		@params.Remove( child );
		InvalidateLayout();
		return RemoveInternalChild( child );
	}

	public void ClearChildren () {
		ClearInternalChildren();
		@params.Clear();
		InvalidateLayout();
	}

	protected void InvalidateLayout () {
		isLayoutInvalidated = true;
		requiredSize = null;
	}
	bool isLayoutInvalidated = true;

	public override void Update () {
		if ( autoSizeDirection != LayoutDirection.None && !isParentLayoutContainer ) {
			autoSizeSelf( new() {
				Width = autoSizeDirection.HasFlag( LayoutDirection.Horizontal ) ? RequiredSize.Width : Size.Width,
				Height = autoSizeDirection.HasFlag( LayoutDirection.Vertical ) ? RequiredSize.Height : Size.Height
			} );
		}

		if ( isLayoutInvalidated ) {
			isLayoutInvalidated = false;
			requiredSize ??= PerformAbsoluteLayout();
			PerformLayout();
		}

		UpdateSubtree();

		Debug.Assert( !isLayoutInvalidated, "The layout could not be settled in 1 update frame." );
	}

	/// <summary>
	/// Performs layout on all (or remaining in case <see cref="PerformAbsoluteLayout"/> already laid some out) children.
	/// </summary>
	protected abstract void PerformLayout ();
	/// <summary>
	/// Calculates required size for children (including padding). 
	/// Optionally at this point layout on known-size elements may be performed.
	/// </summary>
	/// <remarks>
	/// This is guaranteed to be called before <see cref="PerformLayout"/> is invoked.
	/// </remarks>
	protected abstract Size2<float> PerformAbsoluteLayout ();
}