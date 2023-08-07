namespace Vit.Framework.Timing;

public interface IClock {
	double ElapsedTime { get; }
	double CurrentTime { get; }
}
