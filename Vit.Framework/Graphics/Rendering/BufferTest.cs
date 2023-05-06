namespace Vit.Framework.Graphics.Rendering;

public struct BufferTest {
	public bool IsEnabled;
	public bool WriteOnPass;
	public CompareOperation CompareOperation;

	public BufferTest ( CompareOperation compareOperation ) {
		IsEnabled = true;
		WriteOnPass = true;
		CompareOperation = compareOperation;
	}
}

[Flags]
public enum CompareOperation {
	Never = 0x0,
	LessThan = 0x1,
	GreaterThan = 0x2,
	Equal = 0x4,

	NotEqual = LessThan | GreaterThan,
	LessThanOrEqual = LessThan | Equal,
	GreaterThanOrEqual = GreaterThan | Equal,
	Always = LessThan | Equal | GreaterThan
}
