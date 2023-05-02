namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

[Flags]
public enum MemoryOperands : uint {
	None = 0x0,
	Volatile = 0x1,
	Aligned = 0x2,
	Nontemporal = 0x4,
	MakePointerAvailable = 0x8,
	MakePointerVisible = 0x10,
	NonPrivatePointer = 0x20,
	AliasScopeINTELMask = 0x10000,
	NoAliasINTELMask = 0x20000
}
