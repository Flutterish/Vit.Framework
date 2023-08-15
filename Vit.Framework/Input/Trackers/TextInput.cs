using Vit.Framework.Input.Events;

namespace Vit.Framework.Input.Trackers;

public class TextInput : IHasTimestamp {
	public DateTime Timestamp { get; private set; }
	public string LastInput { get; private set; } = string.Empty;

	public abstract class Tracker : InputTracker<Delta, TextInput> {
		TextInput state = new();
		public override TextInput State => state;

		protected override void Update ( Delta update ) {
			state.LastInput = update.Value;
			state.Timestamp = update.Timestamp;
		}

		protected override IEnumerable<TimestampedEvent> EmitEvents ( Delta update ) {
			yield return new TextInputEvent {
				State = state,
				Timestamp = update.Timestamp,
				Text = update.Value
			};
		}
	}

	public struct Delta : IHasTimestamp {
		public required DateTime Timestamp { get; set; }
		public string Value;
	}
}
