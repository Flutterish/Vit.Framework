using System.Numerics;
using System.Runtime.CompilerServices;
using Vit.Framework.Hierarchy;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public abstract partial class Drawable : IDrawable {
	public ICompositeDrawable<Drawable>? Parent { get; internal set; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void trySet<T> ( ref T field, T value ) where T : IEqualityOperators<T, T, bool> {
		if ( field == value )
			return;

		field = value;
		InvalidateDrawNodes();
	}

	Point2<float> position;
	public Point2<float> Position {
		get => position;
		set => trySet( ref position, value );
	}
	public float X {
		get => position.X;
		set => trySet( ref position.X, value );
	}
	public float Y {
		get => position.Y;
		set => trySet( ref position.Y, value );
	}

	Size2<float> size;
	public Size2<float> Size {
		get => size;
		set => trySet( ref size, value );
	}
	public float Width {
		get => size.Width;
		set => trySet( ref size.Width, value );
	}
	public float Height {
		get => size.Height;
		set => trySet( ref size.Height, value );
	}

	Radians<float> rotation;
	public Radians<float> Rotation {
		get => rotation;
		set => trySet( ref rotation, value );
	}

	Point2<float> origin;
	public Point2<float> Origin {
		get => origin;
		set => trySet( ref origin, value );
	}
	public float OriginX {
		get => origin.X;
		set => trySet( ref origin.X, value );
	}
	public float OriginY {
		get => origin.Y;
		set => trySet( ref origin.Y, value );
	}

	Vector2<float> scale = Vector2<float>.One; // TODO dont really like this being a vector
	public Vector2<float> Scale {
		get => scale;
		set => trySet( ref scale, value );
	}
	public float ScaleX {
		get => scale.X;
		set => trySet( ref scale.X, value );
	}
	public float ScaleY {
		get => scale.Y;
		set => trySet( ref scale.Y, value );
	}

	Vector2<float> shear;
	public Vector2<float> Shear {
		get => shear;
		set => trySet( ref shear, value );
	}
	public float ShearX {
		get => shear.X;
		set => trySet( ref shear.X, value );
	}
	public float ShearY {
		get => shear.Y;
		set => trySet( ref shear.Y, value );
	}

	DrawNode?[] drawNodes = new DrawNode?[3];
	protected abstract DrawNode CreateDrawNode ( int subtreeIndex );

	public virtual DrawNode GetDrawNode ( int subtreeIndex ) {
		var node = drawNodes[subtreeIndex] ??= CreateDrawNode( subtreeIndex );
		node.Update();
		return node;
	}
}

public interface IDrawable : IComponent<Drawable> {
	new ICompositeDrawable<Drawable>? Parent { get; }
	IReadOnlyCompositeComponent<Drawable, Drawable>? IComponent<Drawable>.Parent => Parent;
}