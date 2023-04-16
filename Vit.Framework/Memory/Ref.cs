namespace Vit.Framework.Memory;

public class Ref<T> {
	public T Value;

	public Ref ( T value ) {
		Value = value;
	}

	public override string ToString () {
		return $"Ref to {Value}";
	}
}
