using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.UI.Input.Events;

public abstract record TabEvent : UIEvent { }

public record TabbedOverEvent : TabEvent { }

public interface ITabbable : IEventHandler<TabbedOverEvent>, IFocusable {
	bool OnTabbedOver ( TabbedOverEvent @event );

	bool IEventHandler<TabbedOverEvent>.OnEvent ( TabbedOverEvent @event ) {
		return OnTabbedOver( @event );
	}
}
