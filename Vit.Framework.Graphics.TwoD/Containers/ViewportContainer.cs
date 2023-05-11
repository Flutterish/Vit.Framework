﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class ViewportContainer<T> : Container<T> where T : Drawable {
	Size2<float> availableSize;
	public Size2<float> AvailableSize {
		get => availableSize;
		set {
			availableSize = value;
			updateScale();
		}
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
		this.availableSize = availableSize;
		updateScale();
	}

	void updateScale () {
		var aspect = availableSize.Width / availableSize.Height;
		var targetAspect = targetSize.Width / targetSize.Height;

		if ( fillMode == FillMode.Stretch ) {
			ScaleX = availableSize.Width / targetSize.Width;
			ScaleY = availableSize.Height / targetSize.Height;
		}
		else if ( fillMode == FillMode.Fit ) {
			if ( aspect < targetAspect ) {
				ScaleX = availableSize.Width / targetSize.Width;
				ScaleY = availableSize.Height / (targetSize.Width / aspect);
			}
			else {
				ScaleX = availableSize.Width / (targetSize.Height * aspect);
				ScaleY = availableSize.Height / targetSize.Height;
			}
		}
		else if ( fillMode == FillMode.Fill ) {
			if ( aspect >= targetAspect ) {
				ScaleX = availableSize.Width / targetSize.Width;
				ScaleY = availableSize.Height / (targetSize.Width / aspect);
			}
			else {
				ScaleX = availableSize.Width / (targetSize.Height * aspect);
				ScaleY = availableSize.Height / targetSize.Height;
			}
		}
		else if ( fillMode == FillMode.MatchWidth ) {
			ScaleX = availableSize.Width / targetSize.Width;
			ScaleY = availableSize.Height / (targetSize.Width / aspect);
		}
		else if ( fillMode == FillMode.MatchHeight ) {
			ScaleX = availableSize.Width / (targetSize.Height * aspect);
			ScaleY = availableSize.Height / targetSize.Height;
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