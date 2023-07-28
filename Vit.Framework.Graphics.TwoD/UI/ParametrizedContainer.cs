﻿namespace Vit.Framework.Graphics.TwoD.UI;

public class ParametrizedContainer<T, TParam> : CompositeUIComponent<T> where T : UIComponent where TParam : struct {
	List<TParam> parameters = new();

	public virtual IEnumerable<(T child, TParam @param)> LayoutChildren {
		get {
			foreach ( var i in Children ) {
				yield return (i, parameters[i.Depth]);
			}
		}
		set {
			ClearChildren( dispose: true );
			foreach ( var (child, param) in value ) {
				AddChild( child, param );
			}
		}
	}

	public void UpdateLayoutParameters ( T child, TParam param ) {
		parameters[child.Depth] = param;
		InvalidateLayout( LayoutInvalidations.Child );
	}

	public TParam GetLayoutParameters ( T child ) {
		return parameters[child.Depth];
	}

	public void AddChild ( T child, TParam param ) {
		AddInternalChild( child );
		parameters.Add( param );
	}
	public void InsertChild ( int index, T child, TParam param ) {
		InsertInternalChild( index, child );
		parameters.Insert( index, param );
	}

	public void RemoveChild ( T child ) {
		parameters.RemoveAt( child.Depth );
		RemoveInternalChild( child );
	}
	public void RemoveChildAt ( int index ) {
		parameters.RemoveAt( index );
		RemoveInternalChildAt( index );
	}

	public void ClearChildren ( bool dispose ) {
		parameters.Clear();
		ClearInternalChildren( dispose );
	}
}
