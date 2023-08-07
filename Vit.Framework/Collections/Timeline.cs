using System.Diagnostics;

namespace Vit.Framework.Collections;

/// <summary>
/// A seekable, sorted collection where items have start and end times.
/// </summary>
/// <remarks>
/// Events begin on <c>time = startTime</c>, and end on <c>time > endTime</c>. This means 0-duration events last for an infenitesimal amount of time, rather than not at all.
/// </remarks>
public class Timeline<TEvent> {
	static readonly IComparer<(double time, int id)> ascendingComparer = Comparer<(double time, int id)>.Create( static (a,b) => {
		var byTime = a.time.CompareTo( b.time );
		return byTime != 0 ? byTime : a.id.CompareTo( b.id );
	} );

	public int Count => eventsByStartTime.Count;
	public IEnumerable<Event> EventsByStartTime => eventsByStartTime.Values;
	public IEnumerable<Event> EventsByEndTime => eventsByEndTime.Values;

	int nextEventId; // id guarnatees stable sort
	SortedList<(double time, int id), Event> eventsByStartTime = new( ascendingComparer );
	SortedList<(double time, int id), Event> eventsByEndTime = new( ascendingComparer );
	// TODO perhaps this should be a linked list?

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

		var id = nextEventId++;
		var @event = new Event {
			Value = value,
			StartTime = startTime,
			EndTime = endTime,
			Id = id
		};

		if ( SeekBehaviour == SeekBehaviour.Rewind && currentTime >= startTime ) {
			var startingCurrentTime = currentTime;
			seekBefore( startTime );

			eventsByStartTime.Add( (startTime, id), @event );
			eventsByEndTime.Add( (endTime, id), @event );

			SeekTo( startingCurrentTime );
			return @event;
		}

		eventsByStartTime.Add( (startTime, id), @event );
		eventsByEndTime.Add( (endTime, id), @event );

		if ( startTime < currentTime ) {
			nextStartIndex++;
		}
		if ( endTime < currentTime ) {
			nextEndIndex++;
		}

		if ( SeekBehaviour == SeekBehaviour.Acknowledge ) {
			if ( startTime <= currentTime ) {
				EventStarted?.Invoke( @event );
			}
			if ( endTime < currentTime ) {
				EventEnded?.Invoke( @event );
			}
		}

		return @event;
	}

	public void Remove ( Event @event ) {
		if ( SeekBehaviour == SeekBehaviour.Rewind && currentTime >= @event.StartTime ) {
			var startingCurrentTime = currentTime;
			seekBefore( @event.StartTime );

			eventsByStartTime.Remove( (@event.StartTime, @event.Id) );
			eventsByEndTime.Remove( (@event.EndTime, @event.Id) );

			SeekTo( startingCurrentTime );
			return;
		}

		eventsByStartTime.Remove( (@event.StartTime, @event.Id) );
		eventsByEndTime.Remove( (@event.EndTime, @event.Id) );

		if ( @event.StartTime <= currentTime ) {
			nextStartIndex--;
		}
		if ( @event.EndTime < currentTime ) {
			nextEndIndex--;
		}

		if ( SeekBehaviour == SeekBehaviour.Acknowledge ) {
			if ( @event.EndTime < currentTime ) {
				EventEndRewound?.Invoke( @event );
			}
			if ( @event.StartTime <= currentTime ) {
				EventStartRewound?.Invoke( @event );
			}
		}
	}

	int nextStartIndex;
	int nextEndIndex;

	int nextRewindStartIndex => nextStartIndex - 1;
	int nextRewindEndIndex => nextEndIndex - 1;

	double currentTime;
	public double CurrentTime {
		get => currentTime;
		set => SeekTo( value );
	}

	bool isSeeking;
	public void SeekTo ( double goal ) {
		if ( isSeeking )
			throw new InvalidOperationException( "Nested seeking not supported" );
		isSeeking = true;

		if ( goal >= currentTime )
			seekForwardTo( goal );
		else if ( goal < currentTime )
			seekBackwardTo( goal );

		isSeeking = false;
	}

	void seekForwardTo ( double goal ) {
		bool tryEnd ( Event @event ) {
			if ( @event.EndTime >= goal )
				return false;

			nextEndIndex++;
			currentTime = @event.EndTime;
			EventEnded?.Invoke( @event );
			return true;
		}

		bool tryStart ( Event @event ) {
			if ( @event.StartTime > goal )
				return false;

			nextStartIndex++;
			currentTime = @event.StartTime;
			EventStarted?.Invoke( @event );
			return true;
		}

		while ( nextEndIndex < Count ) {
			var nextEnd = eventsByEndTime.Values[nextEndIndex];

			if ( nextStartIndex < Count ) {
				var nextStart = eventsByStartTime.Values[nextStartIndex];

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

			nextEndIndex--;
			currentTime = @event.EndTime;
			EventEndRewound?.Invoke( @event );
			return true;
		}

		bool tryRewindStart ( Event @event ) {
			if ( @event.StartTime <= goal )
				return false;

			nextStartIndex--;
			currentTime = @event.StartTime;
			EventStartRewound?.Invoke( @event );
			return true;
		}

		while ( nextRewindStartIndex >= 0 ) {
			var previousStart = eventsByStartTime.Values[nextRewindStartIndex];

			if ( nextRewindEndIndex >= 0 ) {
				var previousEnd = eventsByEndTime.Values[nextRewindEndIndex];

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

			nextStartIndex--;
			currentTime = @event.StartTime;
			EventStartRewound?.Invoke( @event );
			return true;
		}

		SeekTo( goal );

		isSeeking = true;
		while ( nextRewindStartIndex >= 0 ) {
			var previousStart = eventsByStartTime.Values[nextRewindStartIndex];
			if ( !tryRewindStart( previousStart ) )
				break;
		}

		isSeeking = false;
	}

	public Action<Event>? EventStarted;
	public Action<Event>? EventEnded;
	public Action<Event>? EventStartRewound;
	public Action<Event>? EventEndRewound;

	public record Event {
		public required TEvent Value { get; init; }
		public required double StartTime { get; init; }
		public required double EndTime { get; init; }
		public required int Id { get; init; }

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
	Acknowledge,
	/// <summary>
	/// Rewinds before the inserted/removed event, inserts/removes the event and then seeks back to the previous time again.
	/// </summary>
	Rewind
}