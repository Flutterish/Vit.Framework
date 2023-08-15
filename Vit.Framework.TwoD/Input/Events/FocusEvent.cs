﻿using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public abstract class Focus {
	public abstract void Release ();
}

public record FocusGainedEvent : UIEvent, INonPropagableEvent {
	public required Focus Focus { get; init; }
}

public record FocusLostEvent : UIEvent, INonPropagableEvent {
	public required Focus Focus { get; init; }
}

public interface IFocusable : IEventHandler<FocusGainedEvent>, IEventHandler<FocusLostEvent> {
	bool OnFocused ( FocusGainedEvent @event );

	bool IEventHandler<FocusGainedEvent>.OnEvent ( FocusGainedEvent @event ) {
		return OnFocused( @event );
	}
}