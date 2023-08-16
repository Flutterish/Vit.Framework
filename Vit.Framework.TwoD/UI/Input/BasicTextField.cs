using Vit.Framework.Collections;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Input;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public class BasicTextField : LayoutContainer, IKeyBindingHandler<PlatformAction>, ITextInputHandler, IClickable, IFocusable, ITabable {
	SpriteText spriteText;
	Box selectionBox;
	int cursorIndex => selection.start;
	int selectionStart => int.Min( selection.start, selection.end );
	int selectionEnd => int.Max( selection.start, selection.end );
	int selectionLength => int.Abs( selection.start - selection.end );
	(int start, int end) selection;
	(int start, int end) Selection {
		get => selection;
		set {
			if ( selection == value )
				return;
			
			selection = value;
			updateSelection();
		}
	}

	void updateSelection () {
		var boundingBox = spriteText.Displayed.CalculateBoundingBox( selectionStart, selectionLength );
		if ( selection.start == selection.end ) {
			boundingBox.MinX -= 4;
			boundingBox.MaxX += 4;
		}
		this.Animate().ChangeLayoutParameters( selectionBox, ( LayoutParams v ) => v with {
			Anchor = Anchor<float>.BottomLeft + boundingBox.Position,
			Size = boundingBox.Size
		}, 50.Millis(), Easing.Out );
	}

	int historyIndex;
	RingBuffer<(string text, (int start, int end) selection)> history = new( 64 );
	string Text {
		get => spriteText.Text;
		set {
			spriteText.Text = value;
			changesPending = true;
		}
	}

	bool changesPending;
	void pushHistoryStack () {
		if ( historyIndex != history.Length - 1 ) {
			history.Pop( history.Length - 1 - historyIndex );
		}
		history.Push( (Text, selection) );
		historyIndex++;
		changesPending = false;
		typedWords = false;
	}

	public BasicTextField () {
		AutoSizeDirection = LayoutDirection.Both;
		AddChild( new Box { Tint = ColorRgba.YellowGreen }, new() {
			Size = new(1f.Relative())
		} );
		AddChild( selectionBox = new Box { Tint = ColorRgba.LightBlue }, new() {
			Origin = Anchor.BottomLeft,
			Anchor = Anchor.BottomLeft
		} );
		AddChild( spriteText = new() { FontSize = 32 }, new() {
			Size = (20, 0),
			Anchor = Anchor.CentreLeft,
			Origin = Anchor.CentreLeft
		} );

		history.Push(("", (0,0)));
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		updateSelection();
	}

	bool typedWords;
	public bool OnTextInput ( UITextInputEvent @event ) {
		Text = Text.Substring( 0, selectionStart ) + @event.Text + Text.Substring( selectionEnd );
		var index = selectionStart + @event.Text.Length;
		Selection = (index, index);

		if ( @event.Text.Any( x => !char.IsWhiteSpace( x ) ) )
			typedWords = true;

		if ( typedWords && @event.Text.Any( char.IsWhiteSpace ) )
			pushHistoryStack();

		return true;
	}

	public bool OnClipboardCut ( ClipboardCutEvent @event ) {
		if ( changesPending )
			pushHistoryStack();

		@event.Clipboard.CopyText( selectionLength == 0 ? Text : Text.Substring( selectionStart, selectionLength ) );
		tryDeleteSelection();
		return true;
	}

	public bool OnClipboardCopy ( ClipboardCopyEvent @event ) {
		if ( changesPending )
			pushHistoryStack();

		@event.Clipboard.CopyText( selectionLength == 0 ? Text : Text.Substring( selectionStart, selectionLength ) );
		return true;
	}

	public bool OnClipboardPaste ( ClipboardPasteTextEvent @event ) {
		if ( changesPending )
			pushHistoryStack();

		var pasted = new string( @event.Text.Where( x => !char.IsControl( x ) ).ToArray() );
		Text = Text.Substring( 0, selectionStart ) + pasted + Text.Substring( selectionEnd );
		var index = selectionStart + pasted.Length;
		Selection = (index, index);

		pushHistoryStack();

		return true;
	}

	bool tryDeleteSelection () {
		if ( selectionLength == 0 )
			return false;

		Text = Text.Substring( 0, selectionStart ) + Text.Substring( selectionEnd );
		var index = selectionStart;
		Selection = (index, index);
		pushHistoryStack();
		return true;
	}

	public bool OnKeyDown ( PlatformAction key, bool isRepeat ) {
		switch ( key ) {
			case PlatformAction.MoveBackwardChar:
				var index = int.Max( 0, selectionLength == 0 ? selectionStart - 1 : selectionStart );
				Selection = (index, index);
				break;

			case PlatformAction.MoveForwardChar:
				index = int.Min( Text.Length, selectionLength == 0 ? selectionEnd + 1 : selectionEnd );
				Selection = (index, index);
				break;

			case PlatformAction.SelectBackwardChar:
				index = int.Max( 0, selection.end - 1 );
				Selection = (selection.start, index);
				break;

			case PlatformAction.SelectForwardChar:
				index = int.Min( Text.Length, selection.end + 1 );
				Selection = (selection.start, index);
				break;

			case PlatformAction.DeleteBackwardChar:
				if ( !tryDeleteSelection() && cursorIndex != 0 ) {
					Text = Text.Substring( 0, cursorIndex - 1 ) + Text.Substring( cursorIndex );
					index = cursorIndex - 1;
					Selection = (index, index);
				}
				break;

			case PlatformAction.DeleteForwardChar:
				if ( !tryDeleteSelection() && cursorIndex != Text.Length ) {
					Text = Text.Substring( 0, cursorIndex ) + Text.Substring( cursorIndex + 1 );
				}
				break;

			case PlatformAction.SelectBackwardWord:
			case PlatformAction.MoveBackwardWord:
			case PlatformAction.DeleteBackwardWord:
				if ( key is PlatformAction.DeleteBackwardWord && tryDeleteSelection() )
					break;

				bool seenNonWhitespace = false;
				while ( selection.end != 0 ) {
					selection.end--;
					var isWhitespace = char.IsWhiteSpace( Text[selection.end] );
					if ( !seenNonWhitespace ) {
						seenNonWhitespace = !isWhitespace;
					}
					else if ( isWhitespace ) {
						selection.end++;
						break;
					}
				}

				if ( key is PlatformAction.DeleteBackwardWord ) {
					tryDeleteSelection();
				}
				else if ( key is PlatformAction.MoveBackwardWord ) {
					Selection = (selectionStart, selectionStart);
				}
				else {
					updateSelection();
				}
				break;

			case PlatformAction.SelectForwardWord:
			case PlatformAction.MoveForwardWord:
			case PlatformAction.DeleteForwardWord:
				if ( key is PlatformAction.DeleteForwardWord && tryDeleteSelection() )
					break;

				seenNonWhitespace = false;
				while ( selection.end != Text.Length ) {
					var isWhitespace = char.IsWhiteSpace( Text[selection.end] );
					if ( !seenNonWhitespace ) {
						seenNonWhitespace = !isWhitespace;
					}
					else if ( isWhitespace ) {
						break;
					}

					selection.end++;
				}

				if ( key is PlatformAction.DeleteForwardWord ) {
					tryDeleteSelection();
				}
				else if ( key is PlatformAction.MoveForwardWord ) {
					Selection = (selectionEnd, selectionEnd);
				}
				else {
					updateSelection();
				}
				break;

			case PlatformAction.SelectAll:
				Selection = (0, Text.Length);
				break;

			case PlatformAction.SelectBackwardLine:
			case PlatformAction.DeleteBackwardLine:
			case PlatformAction.MoveBackwardLine:
				if ( key is PlatformAction.DeleteBackwardLine && tryDeleteSelection() )
					break;

				selection.end = 0;
				if ( key is PlatformAction.DeleteBackwardLine ) {
					tryDeleteSelection();
				}
				else if ( key is PlatformAction.MoveBackwardLine ) {
					Selection = (0, 0);
				}
				else {
					updateSelection();
				}
				break;

			case PlatformAction.SelectForwardLine:
			case PlatformAction.DeleteForwardLine:
			case PlatformAction.MoveForwardLine:
				if ( key is PlatformAction.DeleteForwardLine && tryDeleteSelection() )
					break;

				selection.end = Text.Length;
				if ( key is PlatformAction.DeleteForwardLine ) {
					tryDeleteSelection();
				}
				else if ( key is PlatformAction.MoveForwardLine ) {
					Selection = (Text.Length, Text.Length);
				}
				else {
					updateSelection();
				}
				break;

			case PlatformAction.Undo:
				if ( historyIndex == 0 )
					break;

				typedWords = false;
				if ( changesPending )
					pushHistoryStack();

				historyIndex--;
				var stack = history.Peek( history.Length - historyIndex );
				spriteText.Text = stack.text;
				Selection = stack.selection;
				break;

			case PlatformAction.Redo:
				if ( changesPending || historyIndex + 1 == history.Length )
					break;

				typedWords = false;
				historyIndex++;
				stack = history.Peek( history.Length - historyIndex );
				spriteText.Text = stack.text;
				Selection = stack.selection;
				break;

				// TODO also use glyph-size rather than amounf of utf-16 codepoints
				// TODO also use IME

			default:
				return false;
		}

		return true;
	}

	public bool OnKeyUp ( PlatformAction key ) {
		return false;
	}

	Focus? focus;
	public bool OnFocused ( FocusGainedEvent @event ) {
		focus = @event.Focus;
		return true;
	}

	public bool OnFocusLost ( FocusLostEvent @event ) {
		focus = null;
		return true;
	}

	public bool OnTabbedOver ( TabbedOverEvent @event ) {
		return true;
	}

	public bool OnPressed ( PressedEvent @event ) {
		return @event.Button == CursorButton.Left;
	}

	public bool OnReleased ( ReleasedEvent @event ) {
		return true;
	}

	public bool OnClicked ( ClickedEvent @event ) {
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return true;
	}

	protected override void OnUnload () {
		base.OnUnload();
		focus?.Release();
	}
}
