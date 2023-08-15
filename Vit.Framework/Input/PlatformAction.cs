namespace Vit.Framework.Input;

public enum PlatformAction {
	Cut,
	Copy,
	Paste,
	Delete,
	SelectAll,
	Save,
	Undo,
	Redo,
	Exit,
	MoveToListStart,
	MoveToListEnd,
	DocumentNew,
	DocumentPrevious,
	DocumentNext,
	DocumentClose,
	TabNew,
	TabRestore,

	MoveBackwardChar,
	MoveForwardChar,
	DeleteBackwardChar,
	DeleteForwardChar,
	SelectBackwardChar,
	SelectForwardChar,

	MoveBackwardWord,
	MoveForwardWord,
	DeleteBackwardWord,
	DeleteForwardWord,
	SelectBackwardWord,
	SelectForwardWord,

	MoveBackwardLine,
	MoveForwardLine,
	DeleteBackwardLine,
	DeleteForwardLine,
	SelectBackwardLine,
	SelectForwardLine,

	ZoomIn,
	ZoomOut,
	ZoomDefault,

	TabForward,
	TabBackward
}
