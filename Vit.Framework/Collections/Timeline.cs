using System.Diagnostics;

namespace Vit.Framework.Collections;

/// <summary>
/// A seekable, sorted collection where items have start and end times.
/// </summary>
/// <remarks>
/// Events begin on <c>time = startTime</c>, and end on <c>time > endTime</c>. This means 0-duration events last for an infenitesimal amount of time, rather than not at all.
/// </remarks>
public class Timeline<TEvent> {
	static readonly IComparer<double> ascendingComparer = Comparer<double>.Create( static (a,b) => {
		return a.CompareTo( b );
	} );

	public int Count => eventsByStartTime.Count;
	public IEnumerable<Event> EventsByStartTime => eventsByStartTime.Values;
	public IEnumerable<Event> EventsByEndTime => eventsByEndTime.Values;

	SortedLinkedList<double, Event> eventsByStartTime = new( ascendingComparer );
	SortedLinkedList<double, Event> eventsByEndTime = new( ascendingComparer ); // NOTE perhaps we could merge these 2 into one with an start/end flag

	public IEnumerable<Event> EventsAt ( double time ) {
		return eventsByStartTime.Values // TODO this needs some heavy optimisation
			.Where( x => x.StartTime <= time && x.EndTime >= time );
	}

	public IEnumerable<Event> EventsBetween ( double startTime, double endTime ) {
		return eventsByStartTime.Values // TODO this needs some heavy optimisation
			.Where( x => x.EndTime >= startTime && x.StartTime <= endTime );
	}

	public SeekBehaviour SeekBehaviour = SeekBehaviour.Ignore;

	public Event Add ( TEvent value, double startTime, double endTime ) {
		Debug.Assert( endTime >= startTime );

		var @event = new Event {
			Value = value,
			StartTime = startTime,
			EndTime = endTime
		};

		void relink () {
			if ( startTime < currentTime )
				starts.TryReplacePrevious( @event.startNode );
			else
				starts.TryReplaceNext( @event.startNode );

			if ( endTime < currentTime )
				ends.TryReplacePrevious( @event.endNode );
			else
				ends.TryReplaceNext( @event.endNode );
		}

		if ( SeekBehaviour == SeekBehaviour.Rewind && currentTime >= startTime ) {
			var startingCurrentTime = currentTime;
			seekBefore( startTime );

			@event.startNode = eventsByStartTime.AddLast( startTime, @event );
			@event.endNode = eventsByEndTime.AddLast( endTime, @event );
			relink();

			SeekTo( startingCurrentTime );
			return @event;
		}

		@event.startNode = eventsByStartTime.AddLast( startTime, @event );
		@event.endNode = eventsByEndTime.AddLast( endTime, @event );
		relink();

		//if ( SeekBehaviour == SeekBehaviour.Acknowledge ) {
		//	if ( startTime <= currentTime ) {
		//		EventStarted?.Invoke( @event );
		//	}
		//	if ( endTime < currentTime ) {
		//		EventEnded?.Invoke( @event );
		//	}
		//}

		return @event;
	}

	public void Remove ( Event @event ) {
		if ( SeekBehaviour == SeekBehaviour.Rewind && currentTime >= @event.StartTime ) {
			var startingCurrentTime = currentTime;
			seekBefore( @event.StartTime );

			@event.startNode.Remove();
			@event.endNode.Remove();

			SeekTo( startingCurrentTime );
			return;
		}

		@event.startNode.Remove();
		@event.endNode.Remove();

		if ( starts.Next == @event.startNode )
			starts.Next = starts.Previous?.Next;
		if ( ends.Next == @event.endNode )
			ends.Next = ends.Previous?.Next;

		if ( starts.Previous == @event.startNode )
			starts.Previous = starts.Next?.Previous;
		if ( ends.Previous == @event.endNode )
			ends.Previous = ends.Next?.Previous;

		//if ( SeekBehaviour == SeekBehaviour.Acknowledge ) {
		//	if ( @event.EndTime < currentTime ) {
		//		EventEndRewound?.Invoke( @event );
		//	}
		//	if ( @event.StartTime <= currentTime ) {
		//		EventStartRewound?.Invoke( @event );
		//	}
		//}
	}

	record struct Link { // links sit between 2 nodes
		public SortedLinkedList<double, Event>.Node? Next;
		public SortedLinkedList<double, Event>.Node? Previous;

		public void TryReplaceNext ( SortedLinkedList<double, Event>.Node node ) {
			if ( Next == null || Next.Previous == node )
				Next = node;
		}

		public void TryReplacePrevious ( SortedLinkedList<double, Event>.Node node ) {
			if ( Previous == null || Previous.Next == node )
				Previous = node;
		}

		public void MoveForward () { // to move forward Next needs to be not null
			Previous = Next;
			Next = Next!.Next;
		}

		public void MoveBackward () { // to move backward Previous needs to be not null
			Next = Previous;
			Previous = Previous!.Previous;
		}
	}

	Link starts;
	Link ends;

	double currentTime;
	public double CurrentTime {
		get => currentTime;
		set => SeekTo( value );
	}

	public void SeekTo ( double goal ) {
		if ( goal >= currentTime )
			seekForwardTo( goal );
		else if ( goal < currentTime )
			seekBackwardTo( goal );
	}

	void seekForwardTo ( double goal ) {
		bool tryEnd ( Event @event ) {
			if ( @event.EndTime >= goal )
				return false;

			ends.MoveForward();
			currentTime = @event.EndTime;
			EventEnded?.Invoke( @event );
			return true;
		}

		bool tryStart ( Event @event ) {
			if ( @event.StartTime > goal )
				return false;

			starts.MoveForward();
			currentTime = @event.StartTime;
			EventStarted?.Invoke( @event );
			return true;
		}

		while ( ends.Next != null ) {
			var nextEnd = ends.Next.Value;

			if ( starts.Next != null ) {
				var nextStart = starts.Next.Value;

				if ( nextStart.StartTime <= nextEnd.EndTime ) {
					if ( tryStart( nextStart ) )
						continue;
					else
						break;
				}
			}

			if ( !tryEnd( nextEnd ) )
				break;
		}

		currentTime = goal;
	}

	void seekBackwardTo ( double goal ) {
		bool tryRewindEnd ( Event @event ) {
			if ( @event.EndTime < goal )
				return false;

			ends.MoveBackward();
			currentTime = @event.EndTime;
			EventEndRewound?.Invoke( @event );
			return true;
		}

		bool tryRewindStart ( Event @event ) {
			if ( @event.StartTime <= goal )
				return false;

			starts.MoveBackward();
			currentTime = @event.StartTime;
			EventStartRewound?.Invoke( @event );
			return true;
		}

		while ( starts.Previous != null ) {
			var previousStart = starts.Previous.Value;

			if ( ends.Previous != null ) {
				var previousEnd = ends.Previous.Value;

				if ( previousStart.StartTime <= previousEnd.EndTime ) {
					if ( tryRewindEnd( previousEnd ) )
						continue;
					else
						break;
				}
			}

			if ( !tryRewindStart( previousStart ) )
				break;
		}

		currentTime = goal;
	}

	void seekBefore ( double goal ) {
		bool tryRewindStart ( Event @event ) {
			if ( @event.StartTime <= goal )
				return false;

			starts.MoveBackward();
			currentTime = @event.StartTime;
			EventStartRewound?.Invoke( @event );
			return true;
		}

		SeekTo( goal );
		while ( starts.Previous != null ) {
			var previousStart = starts.Previous.Value;
			if ( !tryRewindStart( previousStart ) )
				break;
		}
	}

	public Action<Event>? EventStarted; // NOTE perhaps if these were virtual methods rather than events, it would improve speed?
	public Action<Event>? EventEnded;
	public Action<Event>? EventStartRewound;
	public Action<Event>? EventEndRewound;

	public record Event {
		public required TEvent Value { get; init; }
		public required double StartTime { get; init; }
		public required double EndTime { get; init; }

		internal SortedLinkedList<double, Event>.Node startNode = null!;
		internal SortedLinkedList<double, Event>.Node endNode = null!;

		public double Duration => EndTime - StartTime;
	}
}

/// <summary>
/// Controls how timeline events are handled when an event is inserted/removed behind the current time.
/// </summary>
public enum SeekBehaviour {
	/// <summary>
	/// Events are silently inserted/removed.
	/// </summary>
	Ignore,
	/// <summary>
	/// Fires the start/end events for the event as if using <see cref="Rewind"/>, but only for the inserted/removed event.
	/// </summary>
	//Acknowledge,
	/// <summary>
	/// Rewinds before the inserted/removed event, inserts/removes the event and then seeks back to the previous time again.
	/// </summary>
	Rewind
}