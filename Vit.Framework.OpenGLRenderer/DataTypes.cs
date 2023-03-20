namespace Vit.Framework.Graphics.OpenGl;

public static class DataTypes {
	public static BufferUsageHint Convert ( BufferUsage bufferUsage ) {
		var draw = bufferUsage & BufferUsage.StreamDraw;
		var copy = bufferUsage & BufferUsage.StreamCopy;
		var read = bufferUsage & BufferUsage.StreamRead;

		return (draw, copy, read) switch {
			(BufferUsage.StreamDraw, _, _) => BufferUsageHint.StreamDraw,
			(_, BufferUsage.StreamCopy, _) => BufferUsageHint.StreamCopy,
			(_, _, BufferUsage.StreamRead) => BufferUsageHint.StreamRead,
			(BufferUsage.OftenDraw, _, _) => BufferUsageHint.DynamicDraw,
			(_, BufferUsage.OftenCopy, _) => BufferUsageHint.DynamicCopy,
			(_, _, BufferUsage.OftenRead) => BufferUsageHint.DynamicRead,
			(BufferUsage.RareDraw, _, _) => BufferUsageHint.StaticDraw,
			(_, BufferUsage.RareCopy, _) => BufferUsageHint.StaticCopy,
			(_, _, BufferUsage.RareRead) => BufferUsageHint.StaticRead,
			_ => BufferUsageHint.StaticDraw
		};
	}
}
