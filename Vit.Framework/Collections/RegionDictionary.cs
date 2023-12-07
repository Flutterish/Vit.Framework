using Vit.Framework.Mathematics;

namespace Vit.Framework.Collections;

public class RegionDictionary<TNumber, TCoord, TBucket, TValue, THash> where THash : struct, IDistanceHash<TNumber, TCoord, TBucket> where TBucket : notnull {
	Stack<List<(TValue value, TCoord coord)>> listPool = new();
	Dictionary<TBucket, List<(TValue value, TCoord coord)>> regions = new();

	THash hash;
	public RegionDictionary ( THash hashParams ) {
		hash = hashParams;
	}

	public void Add ( TCoord coord, TValue value ) {
		var bucket = hash.GetBucket( coord );
		if ( !regions.TryGetValue( bucket, out var region ) ) {
			if ( !listPool.TryPop( out region ) )
				region = new();
			regions.Add( bucket, region );
		}

		region.Add( (value, coord) );
	}

	public void Remove ( TCoord coord, TValue value ) {
		regions[hash.GetBucket( coord )].Remove( (value, coord) );
	}

	public void Clear () {
		foreach ( var i in regions ) {
			i.Value.Clear();
			listPool.Push( i.Value );
		}
		regions.Clear();
	}

	public IEnumerable<(TValue value, TCoord coord)> EnumeratePotentiallyInRange ( TCoord center, TNumber range ) {
		foreach ( var bucket in hash.EnumerateBucketsInRange( center, range ) ) {
			if ( !regions.TryGetValue( bucket, out var region ) )
				continue;

			foreach ( var i in region ) {
				yield return i;
			}
		}
	}
}

public interface IDistanceHash<T, TCoord, TBucket> {
	TBucket GetBucket ( TCoord value );
	IEnumerable<TBucket> EnumerateBucketsInRange ( TCoord center, T range );
}

public struct Default2DDistanceHash : IDistanceHash<float, Point2<float>, Point2<int>> {
	public readonly float BucketSizeInverse;
	float bucketSize;
	public required float BucketSize {
		readonly get => bucketSize;
		init {
			bucketSize = value;
			BucketSizeInverse = 1 / value;
		}
	}

	public Point2<int> GetBucket ( Point2<float> value ) {
		return new() {
			X = (int)float.Floor(value.X * BucketSizeInverse),
			Y = (int)float.Floor(value.Y * BucketSizeInverse)
		};
	}

	public IEnumerable<Point2<int>> EnumerateBucketsInRange ( Point2<float> center, float range ) {
		var bottomLeft = GetBucket( center - new Vector2<float>(range, range) );
		var topRight = GetBucket( center + new Vector2<float>(range, range) );

		for ( int x = bottomLeft.X; x <= topRight.X; x++ ) {
			for ( int y = bottomLeft.Y; y <= topRight.Y; y++ ) {
				yield return (x, y);
			}
		}
	}
}
