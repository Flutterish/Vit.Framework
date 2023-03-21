namespace Vit.Framework.Allocation;

public readonly ref struct DisposeAction {
	readonly Action action;

	public DisposeAction ( Action action ) {
		this.action = action;
	}

	public void Dispose () {
		action();
	}
}

public readonly ref struct DisposeAction<T> {
	readonly T param;
	readonly Action<T> action;

	public DisposeAction ( T param, Action<T> action ) {
		this.param = param;
		this.action = action;
	}

	public void Dispose () {
		action( param );
	}
}
