using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Input.Events;

public interface IEventHandler<TEvent> where TEvent : Event {
	bool OnEvent ( TEvent @event );
}
