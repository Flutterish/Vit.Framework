using System.Collections.Immutable;

namespace Vit.Framework.Input;

public struct KeyBinding<TFrom, TTo> where TFrom : struct, Enum where TTo : struct, Enum {
	public readonly TTo Value;
	public readonly ImmutableArray<TFrom> Binding;

	public KeyBinding ( TTo value, params TFrom[] bindings ) {
		Value = value;
		Array.Sort( bindings );
		Binding = bindings.ToImmutableArray();
	}

	public override string ToString () {
		return $"{Value} <- {string.Join(" + ", Binding)}";
	}
}