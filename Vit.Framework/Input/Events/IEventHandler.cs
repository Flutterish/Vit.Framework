namespace Vit.Framework.Input.Events;

public interface IEventHandler<TEvent> where TEvent : Event {
	bool OnEvent ( TEvent @event );
}
