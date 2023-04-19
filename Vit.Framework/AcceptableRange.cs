namespace Vit.Framework;

/// <summary>
/// A range of acceptable values with an explicit ideal value.
/// </summary>
public struct AcceptableRange<T> {
	/// <summary>
	/// The value that will be immediately picked if it is available, 
	/// otherwise the best (as specified by <see cref="SearchMode"/>) available value 
	/// between (inclusive) <see cref="Minimum"/> and (inclusive) <see cref="Maximum"/> will be picked.
	/// </summary>
	public required T Ideal;

	bool minimumSet;
	T minimum;
	/// <summary>
	/// The minimum acceptable value. If not specified, it is equal to <see cref="Ideal"/>.
	/// </summary>
	public T Minimum {
		get => minimumSet ? minimum : Ideal;
		set => (minimum, minimumSet) = (value, true);
	}

	bool maximumSet;
	T maximum;
	/// <summary>
	/// The maximum acceptable value. If not specified, it is equal to <see cref="Ideal"/>.
	/// </summary>
	public T Maximum {
		get => maximumSet ? maximum : Ideal;
		set => (maximum, maximumSet) = (value, true);
	}
	/// <summary>
	/// Specifies how to search for the best value when <see cref="Ideal"/> is not available.
	/// </summary>
	public SearchMode SearchMode;

	public static implicit operator AcceptableRange<T> ( T value ) 
		=> new() { Ideal = value };

	public delegate double DistanceMetric ( T value, T goal );
	public IEnumerable<T> Order ( IEnumerable<T> available, DistanceMetric distanceMetric ) {
		var ideal = Ideal;
		var idealEnumarable = available.Contains( ideal ) ? new[] { ideal } : Array.Empty<T>();

		var min = Minimum;
		var max = Maximum;

		available = available.Where( x => distanceMetric( min, x ) >= 0 && distanceMetric( max, x ) <= 0 );
		if ( SearchMode is SearchMode.ClosestBiasUp or SearchMode.ClosestBiasDown ) {
			var searchMode = SearchMode;
			var ordered = available.Order( Comparer<T>.Create( ( a, b ) => {
				var A = distanceMetric( a, ideal );
				var B = distanceMetric( b, ideal );
				var diff = Math.Abs( A ) - Math.Abs( B );
				return ( diff == 0 && Math.Sign( A ) != Math.Sign( B ) ) ? ( searchMode == SearchMode.ClosestBiasUp ? Math.Sign( A ) : Math.Sign( B ) ) : Math.Sign( diff );
			} ) );

			return idealEnumarable.Concat( ordered );
		}
		else {
			var lower = available.Where( x => distanceMetric( x, ideal ) > 0 ).OrderDescending();
			var higher = available.Where( x => distanceMetric( x, ideal ) < 0 ).Order();

			var ordered = SearchMode == SearchMode.HigherFirst ? higher.Concat( lower ) : lower.Concat( higher );

			return idealEnumarable.Concat( ordered );
		}
	}

	public T? Pick ( IEnumerable<T> available, DistanceMetric distanceMetric ) {
		return Order( available, distanceMetric ).FirstOrDefault();
	}
}

public enum SearchMode {
	/// <summary>
	/// Picks the closest value. If 2 values have equal distance, the higher is picked.
	/// </summary>
	ClosestBiasUp,
	/// <summary>
	/// Picks the closest value. If 2 values have equal distance, the lower is picked.
	/// </summary>
	ClosestBiasDown,
	/// <summary>
	/// Picks the closest value higher than specified. If none are available picks the closest value lower than specified.
	/// </summary>
	HigherFirst,
	/// <summary>
	/// Picks the closest value lower than specified. If none are available picks the closest value higher than specified.
	/// </summary>
	LowerFirst
}