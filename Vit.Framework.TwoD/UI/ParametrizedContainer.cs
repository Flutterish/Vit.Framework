namespace Vit.Framework.TwoD.UI;

public abstract class ParametrizedContainer<T, TParam> : CompositeUIComponent<T> where T : UIComponent where TParam : struct {
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
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		var old = parameters[child.Depth];
		parameters[child.Depth] = param;
		OnChildParameterUpdated( child, old, param );
	}
	public void UpdateLayoutParameters ( T child, Func<TParam, TParam> transformer ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		var old = parameters[child.Depth];
		var param = parameters[child.Depth] = transformer( old );
		OnChildParameterUpdated( child, old, param );
	}

	/// <summary>
	/// Performs the necessary invalidations after a parameter change.
	/// </summary>
	protected abstract void OnChildParameterUpdated ( T child, TParam? previous, TParam? current );

	public TParam GetLayoutParameters ( T child ) {
		return parameters[child.Depth];
	}

	public void AddChild ( T child, TParam param ) {
		AddInternalChild( child );
		parameters.Add( param );
		OnChildParameterUpdated( child, null, param );
	}
	public void InsertChild ( int index, T child, TParam param ) {
		InsertInternalChild( index, child );
		parameters.Insert( index, param );
		OnChildParameterUpdated( child, null, param );
	}

	public void RemoveChild ( T child ) {
		var param = parameters[child.Depth];
		parameters.RemoveAt( child.Depth );
		RemoveInternalChild( child );
		OnChildParameterUpdated( child, param, null );
	}
	public void RemoveChildAt ( int index ) {
		var param = parameters[index];
		parameters.RemoveAt( index );
		var child = Children[index];
		RemoveInternalChildAt( index );
		OnChildParameterUpdated( child, param, null );
	}

	public void ClearChildren ( bool dispose ) {
		parameters.Clear();
		ClearInternalChildren( dispose );
	}
}
