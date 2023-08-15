using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Collections;

public class RingBuffer<T> {
	public readonly int Capacity;
	public int Length { get; private set; }
	int index;
	T[] data;
	public RingBuffer ( int capacity ) {
		Capacity = capacity;
		data = new T[capacity];
	}

	public void Push ( T value ) {
		data[index] = value;
		index = (index + 1) % Capacity;
		Length = int.Min( Length + 1, Capacity );
	}

	public void Pop ( int count ) {
		if ( count > Length || count < 0 )
			throw new InvalidOperationException();
		
		Length -= count;
		index = (index - count).Mod( Capacity );
	}

	public bool TryPop ( [NotNullWhen(true)] out T? value ) {
		if ( Length == 0 ) {
			value = default;
			return false;
		}

		value = data[index]!;
		Length--;
		index = (index - 1).Mod( Capacity );
		return true;
	}

	public T Peek ( int count ) {
		if ( count > Length || count < 0 )
			throw new InvalidOperationException();

		return data[ (index - count).Mod( Capacity ) ];
	}
}
