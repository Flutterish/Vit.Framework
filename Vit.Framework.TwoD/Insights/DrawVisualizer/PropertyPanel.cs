using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

public class PropertyPanel : ScrollContainer<FlowContainer> {
	[SetsRequiredMembers]
	public PropertyPanel () {
		ScrollDirection = LayoutDirection.Both;
		AllowedOverscroll = new() { Bottom = 1f.Relative() - 40 - 30 };
		ContentSize = new( 1f.Relative(), 0 );
		ContentAnchor = Anchor.TopLeft;
		ContentOrigin = Anchor.TopLeft;
		Content = new() {
			AutoSizeDirection = LayoutDirection.Vertical,
			Padding = new( 10 ),
			FlowDirection = FlowDirection.Down
		};
	}

	RenderThreadScheduler disposeScheduler = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		disposeScheduler = dependencies.Resolve<RenderThreadScheduler>();
	}

	const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
	object? target;
	public object? Target {
		get => target;
		set {
			if ( !value.TrySet( ref target ) )
				return;

			textByProperty.Clear();
			textByField.Clear();
			Content.DisposeChildren( disposeScheduler );
			if ( target == null )
				return;

			throwingProperties ??= new();
			var type = target.GetType();
			foreach ( var i in type.GetProperties( flags ).Where( x => x.CanRead && !throwingProperties.Contains( x ) ) ) {
				SpriteText text = new() { FontIdentifier = FrameworkUIScheme.FontCollection, FontSize = 32 };
				Content.AddChild( text, new() {
					Margins = new( 10 )
				} );
				textByProperty.Add( i, text );
			}

			foreach ( var i in type.GetFields( flags ) ) {
				SpriteText text = new() { FontIdentifier = FrameworkUIScheme.FontCollection, FontSize = 32 };
				Content.AddChild( text, new() {
					Margins = new( 10 )
				} );
				textByField.Add( i, text );
			}
		}
	}

	[ThreadStatic]
	static HashSet<PropertyInfo>? throwingProperties;

	Dictionary<PropertyInfo, SpriteText> textByProperty = new();
	Dictionary<FieldInfo, SpriteText> textByField = new();

	public override void Update () {
		foreach ( var (prop, text) in textByProperty ) {
			try {
				text.Tint = ColorRgb.Black;
				var value = prop.GetValue( target );
				if ( value == null )
					text.RawText = $"{prop.Name} = Null";
				else
					text.RawText = $"{prop.Name} = {value}";
			}
			catch ( Exception e ) {
				text.RawText = $"{prop.Name} [!] {e.Message}";
				text.Tint = ColorRgb.Red;
				(throwingProperties ??= new() ).Add( prop );
				textByProperty.Remove( prop );
			}
		}
		foreach ( var (field, text) in textByField ) {
			var value = field.GetValue( target );
			if ( value == null )
				text.RawText = $"{field.Name} = Null";
			else
				text.RawText = $"{field.Name} = {value}";
		}

		base.Update();
	}
}
