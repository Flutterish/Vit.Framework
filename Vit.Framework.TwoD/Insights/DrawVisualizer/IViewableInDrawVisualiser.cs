using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

/// <summary>
/// Indicates that this component can be viewed in the <see cref="DrawVisualizer"/>, and optionally can create a <see cref="DrawVisualizerBlueprint"/> for itself.
/// </summary>
public interface IViewableInDrawVisualiser {
	Matrix3<float> UnitToGlobalMatrix { get; }
	IViewableInDrawVisualiser? Parent { get; }
	IEnumerable<IViewableInDrawVisualiser> Children { get; }
	string Name { get; }

	DrawVisualizerBlueprint? CreateBlueprint ();
}
