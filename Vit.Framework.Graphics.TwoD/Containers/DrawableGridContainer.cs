using System.Diagnostics;
using System.Numerics;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class DrawableGridContainer<T> : DrawableLayoutContainer<T, GridParam> where T : IDrawableLayoutElement {
	GridLines rows = null!;
	public required GridLines Rows {
		get => rows;
		set {
			if ( rows == value )
				return;

			rows = value;
			InvalidateLayout();
		}
	}

	GridLines columns = null!;
	public required GridLines Columns {
		get => columns;
		set {
			if ( columns == value )
				return;

			columns = value;
			InvalidateLayout();
		}
	}

	RelativeAxes2<float> flowOrigin;
	/// <summary>
	/// What point the columns/rows are aligned to when there is unused space in a row/column.
	/// </summary>
	public required RelativeAxes2<float> ContentAlignment {
		get => flowOrigin;
		set {
			if ( flowOrigin == value )
				return;

			flowOrigin = value;
			InvalidateLayout();
		}
	}


	Justification rowJustification;
	/// <summary>
	/// How rows are justified.
	/// </summary>
	public Justification RowJustification {
		get => rowJustification;
		set {
			if ( rowJustification == value )
				return;

			rowJustification = value;
			InvalidateLayout();
		}
	}

	Justification columnJustification;
	/// <summary>
	/// How columns are justified.
	/// </summary>
	public Justification ColumnJustification {
		get => columnJustification;
		set {
			if ( columnJustification == value )
				return;

			columnJustification = value;
			InvalidateLayout();
		}
	}

	FlowDirection flowDirection;
	/// <summary>
	/// Defines the flow/cross axes for grid elements.
	/// It is invalid to specify no cross axis.
	/// </summary>
	public required FlowDirection FlowDirection {
		get => flowDirection;
		set {
			Debug.Assert( value.GetCoveredDirections() == LayoutDirection.Both );
			if ( flowDirection == value )
				return;

			flowDirection = value;
			InvalidateLayout();
		}
	}

	FlowDirection fillDirection;
	/// <summary>
	/// Defines in which direction the grid cells are filled.
	/// It is invalid to specify no cross axis.
	/// </summary>
	public required FlowDirection FillDirection {
		get => fillDirection;
		set {
			Debug.Assert( value.GetCoveredDirections() == LayoutDirection.Both );
			if ( fillDirection == value )
				return;

			fillDirection = value;
			InvalidateLayout();
		}
	}

	Alignment itemFlowAlignment;
	/// <summary>
	/// How elements inside cells are aligned along the flow axis.
	/// </summary>
	public Alignment ItemFlowAlignment {
		get => itemFlowAlignment;
		set {
			if ( itemFlowAlignment == value )
				return;

			itemFlowAlignment = value;
			InvalidateLayout();
		}
	}

	Alignment itemCrossAlignment;
	/// <summary>
	/// How elements inside cells are aligned along the cross axis.
	/// </summary>
	public Alignment ItemCrossAlignment {
		get => itemCrossAlignment;
		set {
			if ( itemCrossAlignment == value )
				return;

			itemCrossAlignment = value;
			InvalidateLayout();
		}
	}

	bool fillGaps;
	/// <summary>
	/// Whether to fill gaps using elements without a specified position.
	/// </summary>
	public bool FillGaps {
		get => fillGaps;
		set {
			if ( fillGaps == value )
				return;

			fillGaps = value;
			InvalidateLayout();
		}
	}

	Size2<float> gap;
	/// <summary>
	/// Gap between grid tracks.
	/// </summary>
	public Size2<float> Gap {
		get => gap;
		set {
			if ( gap == value )
				return;

			gap = value;
			InvalidateLayout();
		}
	}

	protected override Size2<float> PerformAbsoluteLayout () {
		return Size2<float>.Zero;
	}

	GridLayout layout = new();
	protected override void PerformLayout () {
		var contentSize = ContentSize;

		using var points = new RentedArray<Point2<int>>( Children.Count );
		layout.ExpandSize( Rows.TrackSizes.Count, Columns.TrackSizes.Count );
		int index = 0;
		foreach ( var (child, param) in LayoutChildren ) {
			points[index++] = layout.Add( param.RowStart, param.ColumnStart, param.RowCount ?? 1, param.ColumnCount ?? 1 );
		}

		using var columnLines = new RentedArray<float>( layout.ColumnCount * 2 );
		using var rowLines = new RentedArray<float>( layout.RowCount * 2 );
		Columns.GetLineOffsets( columnLines, contentSize.Width, gap.Width );
		Rows.GetLineOffsets( rowLines, contentSize.Height, gap.Height );

		index = 0;
		foreach ( var (child, param) in LayoutChildren ) {
			var point = points[index++];

			var rowStart = rowLines[point.Y * 2];
			var rowEnd = rowLines[point.Y * 2 + 1];
			var columnStart = columnLines[point.X * 2];
			var columnEnd = columnLines[point.X * 2 + 1];

			child.Size = new( columnEnd - columnStart, rowEnd - rowStart );
			child.Position = new( columnStart, rowStart );
		}

		layout.Clear();
	}

	class GridLayout {
		Stack<List<bool>> rowPool = new();

		public int RowCount { get; private set; }
		public int ColumnCount { get; private set; }
		List<List<bool>> rows = new();
		public void Clear () {
			while ( rows.Any() ) {
				var row = rows[^1];
				row.Clear();
				rowPool.Push( row );
				rows.RemoveAt( rows.Count - 1 );
			}

			RowCount = 0;
			ColumnCount = 0;
			cursorX = 0;
			cursorY = 0;
		}

		public void ExpandSize ( int rows, int columns ) {
			RowCount = int.Max( rows, RowCount );
			ColumnCount = int.Max( columns, ColumnCount );
			expand( rows, columns );
		}

		void expand ( int rows, int columns ) {
			while ( this.rows.Count < rows ) {
				if ( !rowPool.TryPop( out var row ) )
					row = new();

				this.rows.Add( row );
			}

			for ( int y = 0; y < rows; y++ ) {
				var _row = this.rows[y];
				while ( _row.Count < columns ) {
					_row.Add( true );
				}
			}
		}

		public bool isFree ( int x, int y ) {
			return rows[y][x];
		}

		public bool isFree ( int x, int y, int columns, int rows ) {
			for ( int dy = 0; dy < rows; dy++ ) {
				for ( int dx = 0; dx < columns; dx++ ) {
					if ( !isFree( x + dx, y + dy ) )
						return false;
				}
			}

			return true;
		}

		void fill ( int x, int y, int columns, int rows ) {
			for ( int dy = 0; dy < rows; dy++ ) {
				for ( int dx = 0; dx < columns; dx++ ) {
					this.rows[y][x] = false;
				}
			}
		}

		int cursorX;
		int cursorY;
		public Point2<int> Add ( int? rowStart, int? columnStart, int rowCount, int columnCount ) {
			if ( rowStart != null || columnStart != null )
				throw new NotImplementedException();

			for ( ; cursorY <= this.RowCount - rowCount; cursorY++ ) {
				for ( ; cursorX <= this.ColumnCount - columnCount; cursorX++ ) {
					if ( isFree( cursorX, cursorY, columnCount, rowCount ) ) {
						goto end;
					}
				}

				cursorX = 0;
			}
		end:

			fill( cursorX, cursorY, columnCount, rowCount );
			return (cursorX, cursorY);
		}
	}
}

public struct GridParam {
	public RelativeSize2<float>? Size;

	public int? RowStart;
	public int? RowCount;
	public int? ColumnStart;
	public int? ColumnCount;
}

public class GridLines {
	/// <summary>
	/// How tracks repeat. Uses <see cref="ImplicitTrackSizes"/> if specified, otherwise uses <see cref="TrackSizes"/>.
	/// </summary>
	public TrackRepeatMode RepeatMode { get; init; }

	/// <summary>
	/// Sizes of required tracks.
	/// </summary>
	public required IReadOnlyList<TrackSize<float>> TrackSizes { get; init; }

	/// <summary>
	/// Sizes of tracks that get created when an element is placed outside the grid defined by required tracks.
	/// </summary>
	/// <remarks>
	/// These track sizes loop.
	/// </remarks>
	public IReadOnlyList<TrackSize<float>>? ImplicitTrackSizes { get; init; }

	public void GetLineOffsets ( Span<float> offsets, float availableSpace, float gap ) {
		var offset = 0f;
		offsets[0] = offset;

		for ( int i = 1; i < offsets.Length; i++ ) {
			var track = TrackSizes[i / 2];
			Debug.Assert( track.Min == null && track.Max == null && track.Base.Type == GridUnitType.Absolute );
			offset += track.Base.Value;
			offsets[i++] = offset;
			if ( i < offsets.Length ) {
				offset += gap;
				offsets[i] = offset;
			}
		}
	}
}

public enum TrackRepeatMode {
	None,
	/// <summary>
	/// Create tracks to fit as many elements as possible.
	/// </summary>
	Fit,
	/// <summary>
	/// Create as many tracks as possible.
	/// </summary>
	Fill
}

public struct TrackSize<T> where T : INumber<T> {
	public GridUnit<T> Base;
	public GridUnit<T>? Min;
	public GridUnit<T>? Max;

	public static implicit operator TrackSize<T> ( T value ) => new() {
		Base = value
	};
}

public struct GridUnit<T> where T : INumber<T> {
	public T Value;
	public GridUnitType Type;

	public GridUnit ( T value, GridUnitType type = GridUnitType.Absolute ) {
		Value = value;
		Type = type;
	}

	public static implicit operator GridUnit<T> ( T value ) => new( value );
}

public enum GridUnitType {
	Absolute,
	Relative,
	FreeSpace
}