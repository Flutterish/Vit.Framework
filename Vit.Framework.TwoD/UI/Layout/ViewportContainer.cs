using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Vit.Framework.TwoD.Layout;

namespace Vit.Framework.TwoD.UI.Layout;

public class ViewportContainer : ViewportContainer<UIComponent> { }
public class ViewportContainer<T> : ParametrizedContainer<T, LayoutParams>, IViewableInDrawVisualiser where T : UIComponent {
	Size2<float> size;
	/// <summary>
	/// The size available to lay out child elements in, in local space.
	/// This accounts for <see cref="Padding"/>.
	/// </summary>
	public Size2<float> ContentSize {
		get => new( size.Width - padding.Horizontal, size.Height - padding.Vertical );
		private set {
			size = value;
			ScaleX = Size.Width / value.Width;
			ScaleY = Size.Height / value.Height;
		}
	}

	Spacing<float> padding;
	/// <summary>
	/// Padding provides spacing between the container edges and elements. 
	/// It is in-set into the container, so that the container layout size does not change because of padding.
	/// </summary>
	/// <remarks>
	/// Padding may be negative in order to display content outside the actual container bounds.
	/// </remarks>
	public Spacing<float> Padding {
		get => padding;
		set {
			padding = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	protected override void OnChildParameterUpdated ( T child, LayoutParams? previous, LayoutParams? current ) {
		InvalidateLayout( LayoutInvalidations.Self );
	}

	protected override Quad2<float> ComputeScreenSpaceQuad () => new AxisAlignedBox2<float>( new (Width / ScaleX, Height / ScaleY ) ) * UnitToGlobalMatrix;
	public override bool ReceivesPositionalInputAt ( Point2<float> screenSpacePosition ) {
		var localSpace = ScreenSpaceToLocalSpace( screenSpacePosition );

		return localSpace.X >= 0
			&& localSpace.Y >= 0
			&& localSpace.X <= ContentSize.Width
			&& localSpace.Y <= ContentSize.Height;
	}

	Size2<float> targetSize;
	public required Size2<float> TargetSize {
		get => targetSize;
		set {
			targetSize = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	FillMode fillMode = FillMode.Fit;
	public FillMode FillMode {
		get => fillMode;
		set {
			fillMode = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	protected override void PerformSelfLayout () {
		updateScale();

		var size = ContentSize;
		var offset = new Vector2<float>( Padding.Left, Padding.Bottom );

		foreach ( var (i, param) in LayoutChildren ) {
			i.Size = param.Size.GetSize( size ).Contain( i.RequiredSize );

			var origin = i.Size * param.Origin;
			var anchor = size * param.Anchor;

			i.Position = (anchor - origin + offset).FromOrigin();
		}
	}

	void updateScale () {
		var aspect = Math.Abs( Size.Width / Size.Height );
		var targetAspect = targetSize.Width / targetSize.Height;

		if ( fillMode == FillMode.Stretch ) {
			ContentSize = targetSize;
		}
		else if ( fillMode == FillMode.Fit ) {
			if ( aspect < targetAspect ) {
				ContentSize = new( targetSize.Width, targetSize.Width / aspect );
			}
			else {
				ContentSize = new( targetSize.Height * aspect, targetSize.Height );
			}
		}
		else if ( fillMode == FillMode.Fill ) {
			if ( aspect >= targetAspect ) {
				ContentSize = new( targetSize.Width, targetSize.Width / aspect );
			}
			else {
				ContentSize = new( targetSize.Height * aspect, targetSize.Height );
			}
		}
		else if ( fillMode == FillMode.MatchWidth ) {
			ContentSize = new( targetSize.Width, targetSize.Width / aspect );
		}
		else if ( fillMode == FillMode.MatchHeight ) {
			ContentSize = new( targetSize.Height * aspect, targetSize.Height );
		}
		else {
			throw new NotImplementedException();
		}
	}

	Matrix3<float> IViewableInDrawVisualiser.UnitToGlobalMatrix => Matrix3<float>.CreateScale( Width / ScaleX, Height / ScaleY ) * UnitToGlobalMatrix;

	protected override MaskingDrawNode<TSpecialisation> CreateDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return new( this, subtreeIndex );
	}

	new protected class MaskingDrawNode<TSpecialisation> : CompositeUIComponent<T>.MaskingDrawNode<TSpecialisation> where TSpecialisation : unmanaged, IRendererSpecialisation {
		public MaskingDrawNode ( CompositeUIComponent<T> source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			base.UpdateState();
			GlobalToUnit *= Matrix3<float>.CreateScale( Source.Scale );
		}
	}
}

public enum FillMode { // TODO pixel-perfect mode
	/// <summary>
	/// The target area will be fully contained inside available space.
	/// </summary>
	Fit,
	/// <summary>
	/// The avaiable space will be fully contained inside target area.
	/// </summary>
	Fill,
	/// <summary>
	/// The target area will match available space (does not perserve aspect ratio).
	/// </summary>
	Stretch,
	/// <summary>
	/// The target and avaialble width will match.
	/// </summary>
	MatchWidth,
	/// <summary>
	/// The target and avaialble height will match.
	/// </summary>
	MatchHeight
}