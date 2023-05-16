using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct LayoutSize<T> where T : INumber<T> {
	public LayoutMode Mode { get; private set; }
	LayoutUnit<T> _a;
	LayoutUnit<T> _b;

	void setMode ( LayoutMode mode ) {
		if ( Mode != LayoutMode.Unset && Mode != mode )
			throw new InvalidOperationException( $"Tried to set a {mode} layout value while another value is {Mode}." );

		Mode = mode;
	}

	public LayoutUnit<T> FlowSize {
		set {
			setMode( LayoutMode.FlowCross );
			_a = value;
		}
	}
	public LayoutUnit<T> CrossSize {
		set {
			setMode( LayoutMode.FlowCross );
			_b = value;
		}
	}
	public FlowSize2<T> FlowCrossSize {
		set {
			setMode( LayoutMode.FlowCross );
			_a = value.Flow;
			_b = value.Cross;
		}
	}

	public LayoutUnit<T> Width {
		set {
			setMode( LayoutMode.Cardinal );
			_a = value;
		}
	}
	public LayoutUnit<T> Height {
		set {
			setMode( LayoutMode.Cardinal );
			_b = value;
		}
	}
	public Size2<T> Size {
		set {
			setMode( LayoutMode.Cardinal );
			_a = value.Width;
			_b = value.Height;
		}
	}

	public Size2<T> GetSize ( FlowDirection flowDirection, Size2<T> availableSpace ) {
		if ( Mode is LayoutMode.Unset )
			return Size2<T>.Zero;

		if ( Mode is LayoutMode.Cardinal || flowDirection.GetFlowDirection() is LayoutDirection.Horizontal )
			return new() { Width = _a.GetValue( availableSpace.Width ), Height = _b.GetValue( availableSpace.Height ) };

		return new() { Width = _b.GetValue( availableSpace.Width ), Height = _a.GetValue( availableSpace.Height ) };
	}

	public FlowSize2<T> GetFlowSize ( FlowDirection flowDirection, FlowSize2<T> availableSpace ) {
		if ( Mode is LayoutMode.Unset )
			return FlowSize2<T>.Zero;

		if ( Mode is LayoutMode.FlowCross || flowDirection.GetFlowDirection() is LayoutDirection.Horizontal )
			return new() { Flow = _a.GetValue( availableSpace.Flow ), Cross = _b.GetValue( availableSpace.Cross ) };

		return new() { Flow = _b.GetValue( availableSpace.Flow ), Cross = _a.GetValue( availableSpace.Cross ) };
	}

	public override string ToString () {
		return $"{Mode} {_a}x{_b}";
	}
}

public enum LayoutMode {
	Unset,
	Cardinal,
	FlowCross
}