using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Collections;

public static class IEnumerableExtensions {
	public static IEnumerable<(T1, T2)> Cartesian<T1, T2> ( this IEnumerable<T1> self, IEnumerable<T2> other ) {
		foreach ( var a in self ) {
			foreach ( var b in other ) {
				yield return (a, b);
			}
		}
	}
}
