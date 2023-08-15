﻿using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public abstract class Focus {
	public abstract void Release ();
}

public record FocusGainedEvent : UIEvent, ILoggableEvent, INonPropagableEvent {
	public required Focus Focus { get; init; }
}

public record FocusLostEvent : UIEvent, ILoggableEvent, INonPropagableEvent {
	public required Focus Focus { get; init; }
}

public interface IFocusable : IEventHandler<FocusGainedEvent>, IEventHandler<FocusLostEvent> {
	bool OnFocused ( FocusGainedEvent @event );
	bool OnFocusLost ( FocusLostEvent @event );

	bool IEventHandler<FocusGainedEvent>.OnEvent ( FocusGainedEvent @event ) {
		return OnFocused( @event );
	}
	bool IEventHandler<FocusLostEvent>.OnEvent ( FocusLostEvent @event ) {
		return OnFocusLost( @event );
	}
}