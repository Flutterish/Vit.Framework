namespace Vit.Framework.Input.Events;

/// <summary>
/// Declares that this type can handle a given event type.
/// </summary>
/// <remarks>
/// <b>Only</b> that type will be handled. If an event of a dreived type happens, this will not be triggered.
/// </remarks>
public interface IEventHandler<TEvent> where TEvent : Event {
	bool OnEvent ( TEvent @event );
}
