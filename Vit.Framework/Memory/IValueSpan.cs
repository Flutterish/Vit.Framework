using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Memory;

public interface IValueSpan<T> : IReadOnlyValueSpan<T> {
	Span<T> AsSpan ();
}

public interface IReadOnlyValueSpan<T> {
	ReadOnlySpan<T> AsReadOnlySpan ();
}
