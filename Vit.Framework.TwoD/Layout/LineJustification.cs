namespace Vit.Framework.TwoD.Layout;

public enum LineJustification {
	/// <summary>
	/// Use ContentAlignment of the container.
	/// </summary>
	ContentAlignment = Justification.ContentAlignment,
	/// <summary>
	/// Put space between elements.
	/// </summary>
	SpaceBetween = Justification.SpaceBetween,
	/// <summary>
	/// Put space around element edges.
	/// </summary>
	SpaceAround = Justification.SpaceAround,
	/// <summary>
	/// Put space between elements and container edges.
	/// </summary>
	SpaceEvenly = Justification.SpaceEvenly,
	/// <summary>
	/// Stretch elements so that they fill the whole axis.
	/// </summary>
	Stretch
}
