using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Textures;

public class Texture : DisposableObject {
	Image<Rgba32>? data;
	public Texture ( Image<Rgba32> data ) {
		this.data = data;
	}

	/// <summary>
	/// The underlying texture - guaranteed to be set on the draw thread.
	/// </summary>
	public ITexture Value = null!;

	public void Update ( IRenderer renderer ) {
		if ( data == null )
			return;

		if ( Value == null )
			Value = renderer.CreateTexture( new( (uint)data.Size.Width, (uint)data.Size.Height ), PixelFormat.RGBA32 );

		using ( var copy = renderer.CreateImmediateCommandBuffer() ) {
			data.DangerousTryGetSinglePixelMemory( out var memory );
			copy.UploadTextureData<Rgba32>( Value, memory.Span );
		}

		data.Dispose();
		data = null;
	}

	protected override void Dispose ( bool disposing ) {
		Value?.Dispose();
	}
}
