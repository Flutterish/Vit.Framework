namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

[Flags]
public enum ImageOperands : uint {
	None = 0x0,
	Bias = 0x1,
	Lod = 0x2,
	Grad = 0x4,
	ConstOffset = 0x8,
	Offset = 0x10,
	ConstOffsets = 0x20,
	Sample = 0x40,
	MinLod = 0x80,
	MakeTexelAvailable = 0x100,
	MakeTexelAvailableKHR = 0x100,
	MakeTexelVisible = 0x200,
	MakeTexelVisibleKHR = 0x200,
	NonPrivateTexel = 0x400,
	NonPrivateTexelKHR = 0x400,
	VolatileTexel = 0x800,
	VolatileTexelKHR = 0x800,
	SignExtend = 0x1000,
	ZeroExtend = 0x2000,
	Nontemporal = 0x4000,
	Offsets = 0x10000
}
