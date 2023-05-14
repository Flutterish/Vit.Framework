using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public abstract class LayoutContainer<T, TParam> : CompositeDrawable<T>, ILayoutContainer<T, TParam, T>, ILayoutElement where T : class, ILayoutElement where TParam : struct {
	Size2<float> size;
	public Size2<float> ContentSize => size;
	public Size2<float> Size {
		get => size;
		set {
			if ( size == value )
				return;

			InvalidateLayout();
			size = value;
		}
	}

	Padding<float> padding;
	public Padding<float> Padding {
		get => padding;
		set {
			padding = value;
			InvalidateLayout();
		}
	}


	public void UpdateLayoutParameter ( T child, TParam param ) {
		@params[child] = param;
		InvalidateLayout();
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
	}
	bool isLayoutInvalidated = true;

	public override void Update () {
		if ( isLayoutInvalidated ) {
			isLayoutInvalidated = false;
			PerformLayout();
		}

		UpdateSubtree();

		Debug.Assert( !isLayoutInvalidated, "The layout could not be settled in 1 update frame." );
	}

	protected abstract void PerformLayout ();
}