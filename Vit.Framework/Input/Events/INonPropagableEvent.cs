using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Input.Events;

/// <summary>
/// A marker interface for events which can only be point-activated and do not propagate down the event tree.
/// </summary>
public interface INonPropagableEvent { }
