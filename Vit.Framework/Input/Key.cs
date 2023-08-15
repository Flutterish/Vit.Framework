namespace Vit.Framework.Input;

public enum Key {
	Q, W, E, R, T, Y, U, I, O, P,
	A, S, D, F, G, H, J, K, L, Z,
	X, C, V, B, N, M, Space,
	Enter, Backspace, Escape,
	F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
	PrintScreen, ScrollLock, Break,
	Grave, Tilde,
	One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero,
	ExclaimationMark, At, Hash, Dollar, Percent, Caret, Ampersand, Asterisk, LeftPerenthesis, RightPerenthesis,
	Minus, Underscore, Equals, Plus,
	Tab, CapsLock, LeftHost, RightHost, LeftSquareBracket, RightSquareBracket, LeftCurlyBracket, RightCurlyBracket,
	Semicolon, Colon, SingleQuote, DoubleQuote, Slash, Backslash, Pipe,
	Comma, Dot, Smaller, Greater, QuestionMark,
	Insert, Home, PageUp, Delete, End, PageDown,
	ArrowUp, ArrowDown, ArrowLeft, ArrowRight,
	NumLock,
	NumpadDivide, NumpadMultiply, NumpadSubstract, NumpadAdd, NumpadEnter,
	NumpadOne, NumpadTwo, NumpadThree, NumpadFour,
	NumpadFive, NumpadSix, NumpadSeven, NumpadEight, NumpadNine, NumpadZero,
	NumpadDot,

	LeftShift, RightShift,
	LeftControl, RightControl,
	Alt, AltGr,
}

public static class KeyExtensions {
	static Dictionary<int, Key> keysByScanCode = new() {
		{ 0x04, Key.A }, { 0x05, Key.B }, { 0x06, Key.C }, { 0x07, Key.D }, { 0x08, Key.E }, { 0x09, Key.F },
		{ 0x0A, Key.G }, { 0x0B, Key.H }, { 0x0C, Key.I }, { 0x0D, Key.J }, { 0x0E, Key.K }, { 0x0F, Key.L },
		{ 0x10, Key.M }, { 0x11, Key.N }, { 0x12, Key.O }, { 0x13, Key.P }, { 0x14, Key.Q }, { 0x15, Key.R },
		{ 0x16, Key.S }, { 0x17, Key.T }, { 0x18, Key.U }, { 0x19, Key.V }, { 0x1A, Key.W }, { 0x1B, Key.X },
		{ 0x1C, Key.Y }, { 0x1D, Key.Z }, { 0x1E, Key.One }, { 0x1F, Key.Two }, { 0x20, Key.Three }, { 0x21, Key.Four },
		{ 0x22, Key.Five }, { 0x23, Key.Six }, { 0x24, Key.Seven }, { 0x25, Key.Eight }, { 0x26, Key.Nine }, { 0x27, Key.Zero },
		{ 0x28, Key.Enter }, { 0x29, Key.Escape }, { 0x2A, Key.Backspace }, { 0x2B, Key.Tab }, { 0x2C, Key.Space }, { 0x2D, Key.Minus },
		{ 0x2E, Key.Equals }, { 0x2F, Key.LeftSquareBracket }, { 0x30, Key.RightSquareBracket }, { 0x31, Key.Backslash }, { 0x33, Key.Semicolon },
		{ 0x34, Key.SingleQuote }, { 0x35, Key.Grave }, { 0x36, Key.Comma }, { 0x37, Key.Dot }, { 0x38, Key.Slash }, { 0x39, Key.CapsLock },
		{ 0x3A, Key.F1 }, { 0x3B, Key.F2 }, { 0x3C, Key.F3 }, { 0x3D, Key.F4 }, { 0x3E, Key.F5 }, { 0x3F, Key.F6 },
		{ 0x40, Key.F7 }, { 0x41, Key.F8 }, { 0x42, Key.F9 }, { 0x43, Key.F10 }, { 0x44, Key.F11 }, { 0x45, Key.F12 },
		{ 0x46, Key.PrintScreen }, { 0x47, Key.ScrollLock }, { 0x48, Key.Break }, { 0x49, Key.Insert }, { 0x4A, Key.Home }, { 0x4B, Key.PageUp },
		{ 0x4C, Key.Delete }, { 0x4D, Key.End }, { 0x4E, Key.PageDown }, { 0x4F, Key.ArrowRight }, { 0x50, Key.ArrowLeft }, { 0x51, Key.ArrowDown },
		{ 0x52, Key.ArrowUp }, { 0x53, Key.NumLock }, { 0x54, Key.NumpadDivide }, { 0x55, Key.NumpadDivide }, { 0x56, Key.NumpadSubstract }, { 0x57, Key.NumpadEnter },
		{ 0x58, Key.NumpadEnter }, { 0x59, Key.NumpadOne }, { 0x5A, Key.NumpadTwo }, { 0x5B, Key.NumpadThree }, { 0x5C, Key.NumpadFour }, { 0x5D, Key.NumpadFive },
		{ 0x5E, Key.NumpadSix }, { 0x5F, Key.NumpadSeven }, { 0x60, Key.NumpadEight }, { 0x61, Key.NumpadNine }, { 0x62, Key.NumpadZero }, { 0x63, Key.NumpadDot },
		{ 0xE1, Key.LeftShift }, { 0xE2, Key.Alt }, { 0xE3, Key.LeftHost }, { 0xE4, Key.RightControl }, { 0xE5, Key.RightShift }, { 0xE6, Key.AltGr }, { 0xE7, Key.RightHost }
	};

	/// <summary>
	/// https://deskthority.net/wiki/Scancode
	/// </summary>
	public static Key? GetKeyByScanCode ( int scancode )
		=> keysByScanCode.TryGetValue( scancode, out var key ) ? key : null;
	
	public static MergedKey ToMerged ( this Key key ) {
		return key switch {
			Key.LeftControl or Key.RightControl => MergedKey.Control,
			Key.LeftShift or Key.RightShift => MergedKey.Shift,
			Key.Alt or Key.AltGr => MergedKey.Alt,
			_ => (MergedKey)key
		};
	}
}