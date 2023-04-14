namespace Vit.Framework.Mathematics;

public interface IInterpolatable<TSelf, TTime> where TSelf : IInterpolatable<TSelf, TTime> {
	TSelf Lerp ( TSelf goal, TTime time );
}
