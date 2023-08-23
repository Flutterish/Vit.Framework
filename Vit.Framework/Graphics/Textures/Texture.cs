using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
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
	public IDeviceTexture2D Value = null!;
	IStagingTexture2D stagingTexture = null!;
	public ITexture2DView View = null!;
	public ISampler Sampler = null!;

	public void Update ( IRenderer renderer ) {
		if ( data == null )
			return;

		using ( var copy = renderer.CreateImmediateCommandBuffer() ) {
			Update( copy );
		}
	}

	public void Update ( ICommandBuffer commands ) {
		if ( data == null )
			return;

		if ( Value == null ) {
			Value = commands.Renderer.CreateDeviceTexture( new( (uint)data.Size.Width, (uint)data.Size.Height ), PixelFormat.Rgba8 );
			View = Value.CreateView();
			Sampler = commands.Renderer.CreateSampler();
		}

		data.DangerousTryGetSinglePixelMemory( out var memory );
		stagingTexture?.Dispose(); // TODO delete this buffer after upload is complete
		stagingTexture = commands.Renderer.CreateStagingTexture( Value.Size, Value.Format );
		stagingTexture.Upload<Rgba32>( memory.Span );
		Size2<uint> size = ((uint)data.Width, (uint)data.Height);
		commands.CopyTexture( stagingTexture, Value, size, (0,0) );

		data.Dispose();
		data = null;
	}

	protected override void Dispose ( bool disposing ) {
		stagingTexture?.Dispose();
		Sampler?.Dispose();
		View?.Dispose();
		Value?.Dispose();
	}
}
