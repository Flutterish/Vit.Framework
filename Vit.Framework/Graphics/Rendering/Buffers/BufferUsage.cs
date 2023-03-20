namespace Vit.Framework.Graphics.Rendering.Buffers;

[Flags]
public enum BufferUsage : byte {
	NeverDraw = 0b_00000000,
	RareDraw = 0b_00000001,
	OftenDraw = 0b_00000010,
	StreamDraw = 0b_00000011,

	NeverRead = 0b_00000000,
	RareRead = 0b_00000100,
	OftenRead = 0b_00001000,
	StreamRead = 0b_00001100,

	NeverCopy = 0b_00000000,
	RareCopy = 0b_00010000,
	OftenCopy = 0b_00100000,
	StreamCopy = 0b_00110000,

	//Never = 0b_00000000,
	//Rare = 0b_01000000,
	//Often = 0b_10000000,
	//Stream = 0b_11000000
}
