namespace Vit.Framework.Input.Events;

/// <summary>
/// A marker interface for events which only propagate up the event tree and never down.
/// </summary>
public interface IUpPropagableEvent : INonPropagableEvent {

}
