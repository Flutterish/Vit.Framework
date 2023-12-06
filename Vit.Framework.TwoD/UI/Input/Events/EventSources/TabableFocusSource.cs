using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Input.Events.EventSources;

public class TabableFocusSource<THandler> where THandler : class, IHasEventTrees<THandler> {
	public required THandler Root { get; init; }

	EventTree<THandler>? currentTabIndex;
	bool isTabFocused;

	public THandler? TabForward ( Millis timestamp ) {
		return tab( timestamp, forward: false ); // the forward is reversed because events are ordered in reverse order
	}
	public THandler? TabBackward ( Millis timestamp ) {
		return tab( timestamp, forward: true );
	}

	THandler? tab ( Millis timestamp, bool forward ) {
		if ( currentTabIndex != null && isTabFocused ) 
			currentTabIndex = forward ? currentTabIndex.NextWithHandler : currentTabIndex.PreviousWithHandler;

		if ( currentTabIndex == null ) {
			if ( !Root.HandledEventTypes.TryGetValue( typeof( TabbedOverEvent ), out var tabTree ) )
				return null;

			if ( !forward ) {
				while ( tabTree.Children.Any() )
					tabTree = tabTree.Children[^1];
				currentTabIndex = tabTree.Handler == null ? tabTree.PreviousWithHandler : tabTree;
			}
			else {
				currentTabIndex = tabTree.Handler == null ? tabTree.NextWithHandler : tabTree;
			}
		}

		var @event = new TabbedOverEvent { Timestamp = timestamp };
		while ( currentTabIndex != null ) {
			if ( currentTabIndex.Handler!( @event ) ) {
				isTabFocused = true;
				return currentTabIndex.Source;
			}

			currentTabIndex = forward ? currentTabIndex.NextWithHandler : currentTabIndex.PreviousWithHandler;
		}

		return null;
	}

	public void ReleaseTabFocus () {
		isTabFocused = false;
	}

	public void SetTabIndex ( EventTree<THandler>? index ) {
		currentTabIndex = index;
	}
}

public static class TabFocusExtensions {
	public static void FindClosestTabIndex<THandler> ( this TabableFocusSource<THandler> @this, THandler? element ) where THandler : class, IHasEventTrees<THandler>, IComponent<THandler> {
		while ( element != null ) {
			if ( element.HandledEventTypes.TryGetValue( typeof( TabbedOverEvent ), out var tabIndex ) && tabIndex.Handler != null ) {
				@this.SetTabIndex( tabIndex );
				return;
			}

			element = (THandler?)element.Parent;
		}

		@this.SetTabIndex( null );
	}
}