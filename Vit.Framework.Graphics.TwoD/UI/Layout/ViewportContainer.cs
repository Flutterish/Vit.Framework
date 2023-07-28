using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.UI.Layout;

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

	protected override void OnLayoutInvalidated () {
		updateScale();
		base.OnLayoutInvalidated();
	}

	Size2<float> targetSize;
	public Size2<float> TargetSize {
		get => targetSize;
		set {
			targetSize = value;
			updateScale();
		}
	}

	FillMode fillMode;
	public FillMode FillMode {
		get => fillMode;
		set {
			fillMode = value;
			updateScale();
		}
	}

	public ViewportContainer ( Size2<float> targetSize, Size2<float> availableSize, FillMode fillMode ) {
		this.targetSize = targetSize;
		this.fillMode = fillMode;
		this.Size = availableSize;
		updateScale();
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