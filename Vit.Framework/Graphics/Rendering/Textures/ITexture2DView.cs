using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Textures;

public interface ITexture2DView : IDisposable {
	ITexture2D Source { get; }
}
