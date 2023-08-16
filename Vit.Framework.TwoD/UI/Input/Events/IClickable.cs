using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.UI.Input.Events;

public interface IClickable : IHandlesPositionalInput, IEventHandler<PressedEvent>, IEventHandler<ReleasedEvent>, IEventHandler<ClickedEvent> {
	bool OnPressed ( PressedEvent @event );
	bool OnReleased ( ReleasedEvent @event );
	bool OnClicked ( ClickedEvent @event );

	bool IEventHandler<PressedEvent>.OnEvent ( PressedEvent @event ) {
		return OnPressed( @event );
	}
	bool IEventHandler<ReleasedEvent>.OnEvent ( ReleasedEvent @event ) {
		return OnReleased( @event );
	}
	bool IEventHandler<ClickedEvent>.OnEvent ( ClickedEvent @event ) {
		return OnClicked( @event );
	}
}
