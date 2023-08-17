using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Collections;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Animations;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input;

namespace Vit.Framework.Tests.VisualTests;

public class TimelineTest : TestScene {
	Timeline<EventBox> timeline = new() { SeekBehaviour = SeekBehaviour.Rewind, CurrentTime = -1 };

	public TimelineTest () {
		Padding = new( all: 10 );
		void entered ( Timeline<EventBox>.Event v ) {
			v.Value.Animate().FlashColour( ColorRgba.HotPink, ColorRgba.Red, 200.Millis(), Easing.Out );
		}
		void exited ( Timeline<EventBox>.Event v ) {
			v.Value.Animate().FadeColour( ColorRgba.Blue, 400.Millis() );
		}

		timeline.EventStarted = entered;
		timeline.EventEndRewound = entered;

		timeline.EventEnded = exited;
		timeline.EventStartRewound = exited;

		Add( 0.1, 0.25 );
		Add( 0.75, 0.9 );

		Add( 0.25, 0.75 );
		Add( 0.4, 0.6 );

		Add( 0.15, 0.35 );
		Add( 0.15, 0.35 );

		AddButton( 0 );
		AddButton( 0.1 );
		AddButton( 0.20 );
		AddButton( 0.25 );
		AddButton( 0.3 );
		AddButton( 0.375 );
		AddButton( 0.5 );
		AddButton( 0.65 );
		AddButton( 0.75 );
		AddButton( 0.9 );
		AddButton( 1 );
	}

	void Add ( double start, double end ) {
		var events = timeline.EventsBetween( start, end );
		var y = events.Any() ? events.Max( x => x.Value.TimelineY ) : 0;
		var box = new EventBox() { Tint = ColorRgba.Gray, TimelineY = y + 1 };

		AddChild( box, new() {
			Origin = Anchor.TopLeft,
			Anchor = Anchor<float>.TopLeft + new RelativeAxes2<float>( ((float)start).Relative(), -y * 24 ),
			Size = new( ((float)(end - start)).Relative(), 20 )
		} );

		timeline.Add( box, start, end );
	}

	void AddButton ( double time ) {
		AddChild( new Box { Tint = ColorRgba.Yellow }, new() {
			Size = new( 2, 1f.Relative() ),
			Origin = Anchor.BottomCentre,
			Anchor = Anchor<float>.BottomLeft + new RelativeAxes2<float>( ((float)time).Relative(), 0 )
		} );
		AddChild( new BasicButton { Clicked = () => timeline.SeekTo( time ) }, new() {
			Size = (50, 50),
			Origin = Anchor.BottomCentre,
			Anchor = Anchor<float>.BottomLeft + new RelativeAxes2<float>( ((float)time).Relative(), 0 )
		} );
	}

	class EventBox : Box {
		[SetsRequiredMembers]
		public EventBox () { }

		public int TimelineY;
	}
}
