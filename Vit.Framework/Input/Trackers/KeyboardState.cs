using Vit.Framework.Input.Events;

namespace Vit.Framework.Input.Trackers;

public class KeyboardState : IHasTimestamp {
	public DateTime Timestamp { get; private set; }

	HashSet<Key> pressedKeys = new();
	public bool IsDown ( Key key ) => pressedKeys.Contains( key );
	public bool IsUp ( Key key ) => !pressedKeys.Contains( key );

	public abstract class Tracker : InputTracker<Delta, KeyboardState> {
		KeyboardState state = new();
		public override KeyboardState State => state;

		protected override void Update ( Delta update ) {
			if ( update.IsDown )
				state.pressedKeys.Add( update.Key );
			else
				state.pressedKeys.Remove( update.Key );
		}

		protected override IEnumerable<TimestampedEvent> EmitEvents ( Delta update ) {
			if ( update.IsDown ) {
				if ( update.IsRepeat )
					yield return new KeyRepeatEvent { Key = update.Key, Timestamp = update.Timestamp, State = state };
				else
					yield return new KeyDownEvent { Key = update.Key, Timestamp = update.Timestamp, State = state };
			}
			else
				yield return new KeyUpEvent { Key = update.Key, Timestamp = update.Timestamp, State = state };
		}
	}

	public struct Delta : IHasTimestamp {
		public required DateTime Timestamp { get; set; }

		public Key Key;
		public bool IsDown;
		public bool IsRepeat;
	}
}
