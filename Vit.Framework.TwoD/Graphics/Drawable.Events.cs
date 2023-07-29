using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Graphics;

public partial class Drawable { // TODO should drawables even have events? they are more of a UI thing
	public Drawable () {
		IHasEventTrees<IDrawable>.AddDeclaredEventHandlers( this, static (d, t, h) => ((Drawable)d).AddEventHandler( t, h ) );
	}

	static Dictionary<Type, EventTree<IDrawable>> nullEventHandlers = new();
	public IReadOnlyDictionary<Type, EventTree<IDrawable>> HandledEventTypes => eventHandlers ?? nullEventHandlers;

	public bool HandlesEventType ( Type type ) => eventHandlers?.ContainsKey( type ) == true;
	Dictionary<Type, EventTree<IDrawable>>? eventHandlers;
	/// <summary>
	/// Adds an event handler for events of type TEvent. The handler should return <see langword="true"/> to stop propagation, <see langword="false"/> otherwise.
	/// </summary>
	/// <remarks>Only the most specific event type will be handled.</remarks>
	protected void AddEventHandler<TEvent> ( Func<TEvent, bool> handler ) where TEvent : Event {
		AddEventHandler( typeof( TEvent ), e => handler( (TEvent)e ) );
	}
	/// <summary>
	/// Adds an event handler for events of given type. The handler should return <see langword="true"/> to stop propagation, <see langword="false"/> otherwise.
	/// </summary>
	/// <remarks>Only the most specific event type will be handled.</remarks>
	protected void AddEventHandler ( Type type, Func<Event, bool> handler ) {
		var tree = GetEventTree( type );
		if ( tree.Handler != null )
			throw new InvalidOperationException( "Can not have multiple handlers for an event type" );
		tree.Handler = handler;
	}

	protected EventTree<IDrawable> GetEventTree ( Type type ) {
		if ( eventHandlers?.TryGetValue( type, out var tree ) == true )
			return tree;

		eventHandlers ??= new();
		tree = new EventTree<IDrawable>() {
			Source = this,
		};
		eventHandlers.Add( type, tree );

		EventHandlerAdded?.Invoke( type, tree );
		return tree;
	}

	protected void RemoveEventHandler<TEvent> () where TEvent : Event {
		RemoveEventHandler( typeof( TEvent ) );
	}
	protected void RemoveEventHandler ( Type type ) {
		if ( eventHandlers?.TryGetValue( type, out var tree ) != true )
			return;

		tree!.Handler = null;
		if ( tree.ShouldBeCulled ) {
			eventHandlers.Remove( type );
			EventHandlerRemoved?.Invoke( type, tree );
		}
	}

	public event Action<Type, EventTree<IDrawable>>? EventHandlerAdded;
	public event Action<Type, EventTree<IDrawable>>? EventHandlerRemoved;
}
