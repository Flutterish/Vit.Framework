using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Memory;

namespace Vit.Framework.Parsing.Binary.Views;

public class BinaryView<T> {
	Ref<Stream> source;
	long offset;
	bool shouldCache;
	public BinaryView ( Ref<Stream> source, long offset, bool cache ) {
		this.offset = offset;
		this.source = source;
		shouldCache = cache;
	}

	T? cachedValue;
	public T Value {
		get {
			if ( !shouldCache ) {
				source.Value.Position = offset;
				return Parse( source.Value );
			}

			if ( cachedValue is T value )
				return value;

			source.Value.Position = offset;
			return cachedValue = Parse( source.Value );
		}
	}

	public static T Parse ( Stream source ) {
		return default!;
	}

	public static implicit operator T ( BinaryView<T> view )
		=> view.Value;
}
