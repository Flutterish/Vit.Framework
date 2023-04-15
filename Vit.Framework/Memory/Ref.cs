using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Memory;

public class Ref<T> {
	public T Value;

	public Ref ( T value ) {
		Value = value;
	}
}
