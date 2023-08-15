using Vit.Framework.Input.Events;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Input.Trackers;

public class CursorState : IHasTimestamp {
	public const int ButtonCount = 5;
	public DateTime Timestamp { get; private set; }

	public Point2<float> LastScreenSpacePosition { get; private set; }
	public Point2<float> ScreenSpacePosition { get; private set; }
	public Vector2<float> DeltaScreenSpacePosition => ScreenSpacePosition - LastScreenSpacePosition;
	// TODO add scroll

	bool[] down = new bool[ButtonCount];
	public bool IsDown ( CursorButton button ) => down[(int)button];

	public abstract class Tracker : InputTracker<Delta, CursorState> {
		CursorState state = new();
		public override CursorState State => state;

		protected override void Update ( Delta update ) {
			state.Timestamp = update.Timestamp;

			if ( update.Type.HasFlag( DeltaType.Buttons ) ) {
				for ( int i = 0; i < ButtonCount; i++ ) {
					if ( !update.ButtonsChanged[i] )
						continue;

					state.down[i] = update.ButtonsDown[i];
				}
			}

			if ( update.Type.HasFlag( DeltaType.Position ) ) {
				state.LastScreenSpacePosition = state.ScreenSpacePosition;
				state.ScreenSpacePosition = update.Position;
			}
		}

		protected override IEnumerable<TimestampedEvent> EmitEvents ( Delta delta ) {
			if ( delta.Type.HasFlag( DeltaType.Position ) ) {
				yield return new CursorMovedEvent {
					Timestamp = state.Timestamp,
					CursorState = state
				};
			}

			if ( delta.Type.HasFlag( DeltaType.Buttons ) ) {
				for ( int i = 0; i < ButtonCount; i++ ) {
					if ( delta.ButtonsChanged[i] ) {
						if ( delta.ButtonsDown[i] ) {
							yield return new CursorButtonPressedEvent {
								Timestamp = state.Timestamp,
								CursorState = state,
								Button = (CursorButton)i
							};
						}
						else {
							yield return new CursorButtonReleasedEvent {
								Timestamp = state.Timestamp,
								CursorState = state,
								Button = (CursorButton)i
							};
						}
					}
				}
			}
		}
	}

	public struct Delta : IHasTimestamp {
		public required DateTime Timestamp { get; set; }

		public required DeltaType Type;
		public Point2<float> Position;
		public FixedSpan5<bool> ButtonsChanged;
		public FixedSpan5<bool> ButtonsDown;
	}

	[Flags]
	public enum DeltaType {
		Position = 1,
		Buttons = 2
	}
}