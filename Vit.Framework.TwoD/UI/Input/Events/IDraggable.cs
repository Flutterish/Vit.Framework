using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.UI.Input.Events;

public interface IDraggable : IHandlesPositionalInput, IEventHandler<PressedEvent>, IEventHandler<DragStartedEvent>, IEventHandler<DraggedEvent>, IEventHandler<DragEndedEvent> {
	bool OnPressed ( PressedEvent @event );
	bool OnDragStarted ( DragStartedEvent @event );
	bool OnDragged ( DraggedEvent @event );
	bool OnDragEnded ( DragEndedEvent @event );

	bool IEventHandler<PressedEvent>.OnEvent ( PressedEvent @event ) {
		return OnPressed( @event );
	}
	bool IEventHandler<DragStartedEvent>.OnEvent ( DragStartedEvent @event ) {
		return OnDragStarted( @event );
	}
	bool IEventHandler<DraggedEvent>.OnEvent ( DraggedEvent @event ) {
		return OnDragged( @event );
	}
	bool IEventHandler<DragEndedEvent>.OnEvent ( DragEndedEvent @event ) {
		return OnDragEnded( @event );
	}
}
