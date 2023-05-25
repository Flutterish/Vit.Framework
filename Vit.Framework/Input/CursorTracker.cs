using Vit.Framework.Interop;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Input;

public class CursorState : IHasTimestamp {
	public const int ButtonCount = 5;
	public DateTime Timestamp { get; private set; }

	public Point2<float> LastPosition { get; private set; }
	public Point2<float> Position { get; private set; }
	public Vector2<float> DeltaPosition => Position - LastPosition;
	// TODO add scroll

	bool[] down = new bool[ButtonCount];
	bool[] pressed = new bool[ButtonCount];
	bool[] released = new bool[ButtonCount];
	public bool IsDown ( MouseButton button ) => down[(int)button];
	public bool WasPressed ( MouseButton button ) => pressed[(int)button];
	public bool WasReleased ( MouseButton button ) => released[(int)button];

	public abstract class Tracker : InputTracker<Delta, CursorState> {
		CursorState state = new();
		public override CursorState State => state;

		protected override void Update ( Delta update ) {
			state.Timestamp = update.Timestamp;

			if ( update.Type.HasFlag( DeltaType.Buttons ) ) {
				for ( int i = 0; i < ButtonCount; i++ ) {
					if ( !update.ButtonsChanged[i] )
						continue;

					state.released[i] = state.down[i] && !update.ButtonsDown[i];
					state.pressed[i] = !state.down[i] && update.ButtonsDown[i];
					state.down[i] = update.ButtonsDown[i];
				}
			}
			
			if ( update.Type.HasFlag( DeltaType.Position ) ) {
				state.LastPosition = state.Position;
				state.Position = update.Position;
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