using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public interface IHoverable : IHandlesPositionalInput, IEventHandler<CursorEnteredEvent>, IEventHandler<CursorExitedEvent> {
	bool OnCursorEntered ( CursorEnteredEvent @event );
	bool OnCursorExited ( CursorExitedEvent @event );

	bool IEventHandler<CursorEnteredEvent>.OnEvent ( CursorEnteredEvent @event ) {
		return OnCursorEntered( @event );
	}
	bool IEventHandler<CursorExitedEvent>.OnEvent ( CursorExitedEvent @event ) {
		return OnCursorExited( @event );
	}
}
