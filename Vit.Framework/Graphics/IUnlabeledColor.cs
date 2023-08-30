using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics;

public interface IUnlabeledColor<T> where T : INumber<T> {
	ReadOnlySpan<T> AsSpan ();
}
