namespace Vit.Framework.TwoD.UI.Input;

public interface IHasInputValue<out T> {
	T InputValue { get; }
}
