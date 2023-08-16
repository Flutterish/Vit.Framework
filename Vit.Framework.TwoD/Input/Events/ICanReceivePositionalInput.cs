using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Input.Events;

public interface ICanReceivePositionalInput {
	bool ReceivesPositionalInputAt ( Point2<float> screenSpacePosition );
}
