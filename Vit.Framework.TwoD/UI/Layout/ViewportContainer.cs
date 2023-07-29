using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Layout;

public class ViewportContainer : ViewportContainer<UIComponent> { }
public class ViewportContainer<T> : Container<T> where T : UIComponent {
	Size2<float> size;
	public Size2<float> ContentSize {
		get => size;
		private set {
			size = value;
			ScaleX = Size.Width / value.Width;
			ScaleY = Size.Height / value.Height;
		}
	}

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

	protected override void PerformLayout () {
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

		base.PerformLayout();
	}
}

public enum FillMode {
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