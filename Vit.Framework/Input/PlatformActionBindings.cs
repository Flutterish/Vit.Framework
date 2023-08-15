namespace Vit.Framework.Input;

public abstract class PlatformActionBindings {
	public PlatformActionBindings () {
		converter.Bindings = CreatePlatformBindings();
	}

	Converter converter = new();
	public abstract IEnumerable<KeyBinding<MergedKey, PlatformAction>> CreatePlatformBindings ();
	

	class Converter : KeyBindingConverter<MergedKey, PlatformAction> {
		protected override IEnumerable<MergedKey> CreatePersistent ( IEnumerable<MergedKey> all ) {
			return all.Where( x => x is MergedKey.Shift or MergedKey.Control or MergedKey.Alt or MergedKey.Host );
		}
	}

	HashSet<Key> pressed = new();
	Dictionary<MergedKey, int> translated = new();
	public void Add ( Key value ) {
		if ( !pressed.Add( value ) )
			return;

		var merged = value.ToMerged();
		if ( translated.ContainsKey( merged ) ) {
			translated[merged]++;
		}
		else {
			translated[merged] = 1;
			converter.Add( merged );
		}
	}

	public void Remove ( Key value ) {
		if ( !pressed.Remove( value ) )
			return;

		var merged = value.ToMerged();
		translated[merged]--;
		if ( translated[merged] == 0 ) {
			translated.Remove( merged );
			converter.Remove( merged );
		}
	}

	public void Repeat ( Key value ) {
		if ( !pressed.Contains( value ) )
			return;

		var merged = value.ToMerged();
		converter.Repeat( merged );
	}

	public IEnumerable<PlatformAction> PressedBindings => converter.PressedBindings;
	public IEnumerable<Key> PressedInputs => pressed;

	public event Action<PlatformAction>? Pressed {
		add => converter.Pressed += value;
		remove => converter.Pressed -= value;
	}
	public event Action<PlatformAction>? Repeated {
		add => converter.Repeated += value;
		remove => converter.Repeated -= value;
	}
	public event Action<PlatformAction>? Released {
		add => converter.Released += value;
		remove => converter.Released -= value;
	}
}

public class DefaultPlatformActionBindings : PlatformActionBindings {
	public override IEnumerable<KeyBinding<MergedKey, PlatformAction>> CreatePlatformBindings () => new KeyBinding<MergedKey, PlatformAction>[] {
		new( PlatformAction.Cut, new[] { MergedKey.Control, MergedKey.X } ),
		new( PlatformAction.Copy, new[] { MergedKey.Control, MergedKey.C } ),
		new( PlatformAction.Paste, new[] { MergedKey.Control, MergedKey.V } ),
		new( PlatformAction.Cut, new[] { MergedKey.Shift, MergedKey.Delete } ),
		new( PlatformAction.Copy, new[] { MergedKey.Control, MergedKey.Insert } ),
		new( PlatformAction.Paste, new[] { MergedKey.Shift, MergedKey.Insert } ),
		new( PlatformAction.SelectAll, new[] { MergedKey.Control, MergedKey.A } ),
		new( PlatformAction.MoveBackwardChar, MergedKey.ArrowLeft ),
		new( PlatformAction.MoveForwardChar, MergedKey.ArrowRight ),
		new( PlatformAction.DeleteBackwardChar, MergedKey.Backspace ),
		new( PlatformAction.DeleteForwardChar, MergedKey.Delete ),
		new( PlatformAction.SelectBackwardChar, new[] { MergedKey.Shift, MergedKey.ArrowLeft } ),
		new( PlatformAction.SelectForwardChar, new[] { MergedKey.Shift, MergedKey.ArrowRight } ),
		new( PlatformAction.DeleteBackwardChar, new[] { MergedKey.Shift, MergedKey.Backspace } ),
		new( PlatformAction.MoveBackwardWord, new[] { MergedKey.Control, MergedKey.ArrowLeft } ),
		new( PlatformAction.MoveForwardWord, new[] { MergedKey.Control, MergedKey.ArrowRight } ),
		new( PlatformAction.DeleteBackwardWord, new[] { MergedKey.Control, MergedKey.Backspace } ),
		new( PlatformAction.DeleteForwardWord, new[] { MergedKey.Control, MergedKey.Delete } ),
		new( PlatformAction.SelectBackwardWord, new[] { MergedKey.Control, MergedKey.Shift, MergedKey.ArrowLeft } ),
		new( PlatformAction.SelectForwardWord, new[] { MergedKey.Control, MergedKey.Shift, MergedKey.ArrowRight } ),
		new( PlatformAction.MoveBackwardLine, MergedKey.Home ),
		new( PlatformAction.MoveForwardLine, MergedKey.End ),
		new( PlatformAction.SelectBackwardLine, new[] { MergedKey.Shift, MergedKey.Home } ),
		new( PlatformAction.SelectForwardLine, new[] { MergedKey.Shift, MergedKey.End } ),
		new( PlatformAction.DocumentPrevious, new[] { MergedKey.Control, MergedKey.PageUp } ),
		new( PlatformAction.DocumentNext, new[] { MergedKey.Control, MergedKey.PageDown } ),
		new( PlatformAction.DocumentNext, new[] { MergedKey.Control, MergedKey.Tab } ),
		new( PlatformAction.DocumentPrevious, new[] { MergedKey.Control, MergedKey.Shift, MergedKey.Tab } ),
		new( PlatformAction.DocumentClose, new[] { MergedKey.Control, MergedKey.W } ),
		new( PlatformAction.DocumentClose, new[] { MergedKey.Control, MergedKey.F4 } ),
		new( PlatformAction.DocumentNew, new[] { MergedKey.Control, MergedKey.N } ),
		new( PlatformAction.TabNew, new[] { MergedKey.Control, MergedKey.T } ),
		new( PlatformAction.TabRestore, new[] { MergedKey.Control, MergedKey.Shift, MergedKey.T } ),
		new( PlatformAction.Save, new[] { MergedKey.Control, MergedKey.S } ),
		new( PlatformAction.MoveToListStart, MergedKey.Home ),
		new( PlatformAction.MoveToListEnd, MergedKey.End ),
		new( PlatformAction.Undo, new[] { MergedKey.Control, MergedKey.Z } ),
		new( PlatformAction.Redo, new[] { MergedKey.Control, MergedKey.Y } ),
		new( PlatformAction.Redo, new[] { MergedKey.Control, MergedKey.Shift, MergedKey.Z } ),
		new( PlatformAction.Delete, MergedKey.Delete ),
		new( PlatformAction.ZoomIn, new[] { MergedKey.Control, MergedKey.Plus } ),
		new( PlatformAction.ZoomIn, new[] { MergedKey.Control, MergedKey.NumpadAdd } ),
		new( PlatformAction.ZoomOut, new[] { MergedKey.Control, MergedKey.Minus } ),
		new( PlatformAction.ZoomOut, new[] { MergedKey.Control, MergedKey.NumpadSubstract } ),
		new( PlatformAction.ZoomDefault, new[] { MergedKey.Control, MergedKey.Zero } ),
		new( PlatformAction.ZoomDefault, new[] { MergedKey.Control, MergedKey.NumpadZero } )
	};
}