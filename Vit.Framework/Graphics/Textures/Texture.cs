using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Textures;

public class Texture : IDisposable {
	Image<Rgba32> data; // TODO this is kept so switching backends preserves texture data. We need a mechanism for recreating the data instead of hogging memory
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

		if ( Value != null )
			return;

		Value = commands.Renderer.CreateDeviceTexture( new( (uint)data.Size.Width, (uint)data.Size.Height ), PixelFormat.Rgba8 );
		View = Value.CreateView();
		Sampler = commands.Renderer.CreateSampler();

		data.DangerousTryGetSinglePixelMemory( out var memory );
		// TODO detect if image is premultiplied
		// TODO detect image gamma
		stagingTexture = commands.Renderer.CreateStagingTexture( Value.Size, Value.Format ); // TODO delete this buffer after upload is complete
		foreach ( ref var i in memory.Span ) {
			i = i.BitCast<Rgba32, ColorRgba<byte>>().ToSRgb().BitCast<ColorSRgba<byte>, Rgba32>();
		}
		stagingTexture.Upload<Rgba32>( memory.Span );
		Size2<uint> size = ((uint)data.Width, (uint)data.Height);
		commands.CopyTexture( stagingTexture, Value, size, (0,0) );

		//data.Dispose();
		//data = null;
	}

	public void Dispose () {
		stagingTexture?.Dispose();
		Sampler?.Dispose();
		View?.Dispose();
		Value?.Dispose();

		Value = null!;
	}
}
