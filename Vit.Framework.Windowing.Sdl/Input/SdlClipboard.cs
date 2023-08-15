using SDL2;
using Vit.Framework.Input;

namespace Vit.Framework.Windowing.Sdl.Input;


public class SdlClipboard : Clipboard {

	List<ClipboardContent> contents = new() { ClipboardContent.Text };
	public override IReadOnlyList<ClipboardContent> ClipboardContents => SDL.SDL_HasClipboardText() == SDL.SDL_bool.SDL_TRUE
		? contents
		: Array.Empty<ClipboardContent>();

	public override bool CopyText ( string value ) {
		return SDL.SDL_SetClipboardText( value ) == 0;
	}

	public override string? GetText ( int index ) {
		 return SDL.SDL_GetClipboardText();
	}
}
