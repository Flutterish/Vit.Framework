using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public interface ID3D11BufferHandle {
	ID3D11Buffer Handle { get; }
	ID3D11ShaderResourceView? ResourceView { get; }
}
