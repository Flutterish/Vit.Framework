using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public abstract record TabEvent : UIEvent {
	public required Focus Focus { get; init; }
}

public record TabFocusGainedEvent : TabEvent { }

public interface ITabbable : IEventHandler<TabFocusGainedEvent>, IEventHandler<FocusLostEvent> {
	bool OnTabbedOver ( TabFocusGainedEvent @event );

	bool IEventHandler<TabFocusGainedEvent>.OnEvent ( TabFocusGainedEvent @event ) {
		return OnTabbedOver( @event );
	}
}
