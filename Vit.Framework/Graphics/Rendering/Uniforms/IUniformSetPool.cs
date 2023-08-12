using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Rendering.Uniforms;

/// <summary>
/// A pool of <see cref="IUniformSet"/>s.
/// </summary>
public interface IUniformSetPool {
	IUniformSet Rent ();
	void Free ( IUniformSet set );
}
