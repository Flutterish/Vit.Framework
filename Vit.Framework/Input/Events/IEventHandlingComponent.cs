namespace Vit.Framework.Input.Events;

public interface IEventHandlingComponent {
	IEnumerable<KeyValuePair<Type, Func<Event, bool>>> HandledEventTypes { get; }

	event Action<IEventHandlingComponent, Type, Func<Event, bool>>? EventHandlerAdded;
	event Action<IEventHandlingComponent, Type>? EventHandlerRemoved;

	bool OnEvent ( Event @event );
}