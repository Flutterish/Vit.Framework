namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// Specifies how a buffer will be used - this allows for optimized memory allocation.
/// <list type="table">
///		<item><c>Never*</c> - This operation is never performed, or only a few times in its lifetime.</item>
///		<item><c>Rare*</c> - This operation is performed one time per a few seconds.</item>
///		<item><c>Often*</c> - This operation is performed once per 100-ish milliseconds.</item>
///		<item><c>Stream*</c> - This operation is performed about once per frame.</item>
/// </list>
/// <list type="table">
///		<item><c>*Draw</c> - Used for rendering.</item>
///		<item><c>*Upload</c> - Data will be uploaded from the cpu.</item>
///		<item><c>*Read</c> - Used as a copy source.</item>
///		<item><c>*Copy</c> - Used as a copy destination.</item>
/// </list>
/// </summary>
[Flags]
public enum BufferUsage {
	NeverDraw    = 0,
	RareDraw     = 1,
	OftenDraw    = 2,
	StreamDraw   = 3,

	NeverUpload  = 0,
	RareUpload   = 1 << 2,
	OftenUpload  = 2 << 2,
	StreamUpload = 3 << 2,

	NeverRead    = 0,
	RareRead     = 1 << 4,
	OftenRead    = 2 << 4,
	StreamRead   = 3 << 4,

	NeverCopy    = 0,
	RareCopy     = 1 << 6,
	OftenCopy    = 2 << 6,
	StreamCopy   = 3 << 6
}
