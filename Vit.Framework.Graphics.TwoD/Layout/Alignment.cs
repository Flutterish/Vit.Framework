namespace Vit.Framework.Graphics.TwoD.Layout;

public enum Alignment {
	/// <summary>
	/// Align the start of the element with the start of the cross axis of a line.
	/// </summary>
	Start,
	/// <summary>
	/// Align the end of the element with the end of the cross axis of a line.
	/// </summary>
	End,
	/// <summary>
	/// Align the center of the element with the center of the cross axis of a line.
	/// </summary>
	Center,
	/// <summary>
	/// Stretch elements so that they fill the whole cross axis of a line. This ignores max size.
	/// </summary>
	Stretch
}
