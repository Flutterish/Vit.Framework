using System.Reflection;
using Vit.Framework.Input.Events;

namespace Vit.Framework.Graphics.TwoD;

public partial class Drawable {
	[ThreadStatic]
	static Dictionary<Type, Action<Drawable>>? typeInitializer;
	static void addHandler<TEvent> ( Drawable drawable ) where TEvent : Event {
		drawable.AddEventHandler<TEvent>( ((IEventHandler<TEvent>)drawable).OnEvent );
	}

	[ThreadStatic]
	static object[]? initializerParams;
	public Drawable () {
		var type = GetType();
		if ( !(typeInitializer ??= new()).TryGetValue( type, out var action ) ) {
			var interfaces = type.GetInterfaces().Where( x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof( IEventHandler<> ) );
			var eventTypes = interfaces.Select( x => x.GenericTypeArguments[0] );
			
			var adder = typeof(Drawable).GetMethod( nameof(addHandler), BindingFlags.Static | BindingFlags.NonPublic )!;
			var adders = eventTypes.Select( x => adder.MakeGenericMethod( x ) );

			typeInitializer.Add( type, action = drawable => { // TODO this should probably be source generated
				(initializerParams ??= new object[1])[0] = drawable;
				foreach ( var i in adders ) {
					i.Invoke( null, initializerParams );
				}
			} );
		}

		action( this );
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
		if ( tree.Children?.Any() != true ) {
			eventHandlers.Remove( type );
			EventHandlerRemoved?.Invoke( type, tree );
		}
	}

	public event Action<Type, EventTree<IDrawable>>? EventHandlerAdded;
	public event Action<Type, EventTree<IDrawable>>? EventHandlerRemoved;
}
