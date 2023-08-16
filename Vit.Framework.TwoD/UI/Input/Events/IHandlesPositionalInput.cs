using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.UI.Input.Events;

public interface IHandlesPositionalInput : IEventHandler<HoveredEvent> {
	bool OnHovered ( HoveredEvent @event );

	bool IEventHandler<HoveredEvent>.OnEvent ( HoveredEvent @event ) {
		return OnHovered( @event );
	}
}
