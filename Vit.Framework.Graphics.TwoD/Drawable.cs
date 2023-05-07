using Vit.Framework.Hierarchy;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public abstract partial class Drawable : IDrawable {
	public ICompositeDrawable<Drawable>? Parent { get; internal set; }

	Point2<float> position;
	public Point2<float> Position {
		get => position;
		set => position = value;
	}
	public float X {
		get => position.X;
		set => position.X = value;
	}
	public float Y {
		get => position.Y;
		set => position.Y = value;
	}

	Size2<float> size;
	public Size2<float> Size {
		get => size;
		set => size = value;
	}
	public float Width {
		get => size.Width;
		set => size.Width = value;
	}
	public float Height {
		get => size.Height;
		set => size.Height = value;
	}

	Radians<float> rotation;
	public Radians<float> Rotation {
		get => rotation;
		set => rotation = value;
	}

	Point2<float> origin;
	public Point2<float> Origin {
		get => origin;
		set => origin = value;
	}
	public float OriginX {
		get => origin.X;
		set => origin.X = value;
	}
	public float OriginY {
		get => origin.Y;
		set => origin.Y = value;
	}

	Vector2<float> scale = Vector2<float>.One; // TODO dont really like this being a vector
	public Vector2<float> Scale {
		get => scale;
		set => scale = value;
	}
	public float ScaleX {
		get => scale.X;
		set => scale.X = value;
	}
	public float ScaleY {
		get => scale.Y;
		set => scale.Y = value;
	}

	Vector2<float> shear;
	public Vector2<float> Shear {
		get => shear;
		set => shear = value;
	}
	public float ShearX {
		get => shear.X;
		set => shear.X = value;
	}
	public float ShearY {
		get => shear.Y;
		set => shear.Y = value;
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