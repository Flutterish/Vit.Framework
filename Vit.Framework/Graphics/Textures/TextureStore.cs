﻿using System.Collections.Concurrent;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Textures;

public class TextureStore : IDisposable {
	ConcurrentQueue<Texture> texturesToUpload = new();

	Dictionary<TextureIdentifier, Texture> textures = new();

	public TextureStore () {
		var whitePixel = new Image<Rgba32>( 1, 1, new Rgba32( 1f, 1f, 1f ) );
		AddTexture( WhitePixel, new( whitePixel ) );
	}

	public static readonly TextureIdentifier WhitePixel = new() { Name = "White Pixel" };
	public void AddTexture ( TextureIdentifier id, Texture texture ) {
		textures.Add( id, texture );
		texturesToUpload.Enqueue( texture );
	}

	public Texture GetTexture ( TextureIdentifier id ) {
		return textures[id];
	}

	public void UploadNew ( IRenderer renderer ) {
		if ( !texturesToUpload.Any() )
			return;

		using var commands = renderer.CreateImmediateCommandBuffer();
		while ( texturesToUpload.TryDequeue( out var texture ) ) {
			texture.Update( commands );
		}
	}

	public void Dispose () {
		foreach ( var (_, i) in textures ) {
			texturesToUpload.Enqueue( i );
			i.Dispose();
		}
	}
}

public class TextureIdentifier {
	public required string Name;
}