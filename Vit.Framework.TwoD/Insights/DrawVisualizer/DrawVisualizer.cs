using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input.Events;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

/// <summary>
/// A debug screen overlay which allows inspecting the draw hierarchy. Includes ability to visually edit components via "blueprints".
/// </summary>
public class DrawVisualizer : LayoutContainer, IGlobalKeyBindingHandler<Input.Key>, IClickable {
	DrawHierarchyVisualizer hierarchy;
	PropertyPanel properties;
	DraggableContainer container;
	DrawVisualizerCursor cursor;

	UIComponent root;
	public DrawVisualizer ( UIComponent root ) {
		this.root = root;

		AddChild( cursor = new(), new() );
		container = new() {
			Dragged = delta => {
				UpdateLayoutParameters( container!, delta, ( param, delta ) => param with { Anchor = param.Anchor + ScreenSpaceDeltaToLocalSpace( delta ) } );
			},
			AutoSizeDirection = LayoutDirection.Horizontal,
			Content = new LayoutContainer {
				AutoSizeDirection = LayoutDirection.Horizontal,
				LayoutChildren = new (UIComponent, LayoutParams)[] {
					(new Box { Tint = FrameworkUIScheme.Background }, new() { Size = new(1f.Relative()) }),
					(new FlowContainer() {
						AutoSizeDirection = LayoutDirection.Horizontal,
						FlowDirection = FlowDirection.Right,
						LayoutChildren = new (UIComponent, FlowParams)[] {
							(hierarchy = new(), new() {
								Size = new() {
									Base = new( 800, 1f.Relative() )
								}
							}),
							(properties = new(), new() {
								Size = new() {
									Base = new( 800, 1f.Relative() )
								}
							})
						}
					}, new() { Size = new(1f.Relative()) })
				}
			}
		};

		hierarchy.Selected = t => {
			cursor.Target = t;
			properties.Target = t;
		};
	}

	public void View ( IViewableInDrawVisualiser? target ) {
		hierarchy.View( target );
	}

	bool isSelecting;
	bool IsSelecting {
		get => isSelecting;
		set {
			if ( !value.TrySet( ref isSelecting ) )
				return;

			cursor.Target = value ? this : null;
		}
	}
	public bool OnKeyDown ( Key key, bool isRepeat ) {
		if ( key == Key.F1 && !isRepeat ) {
			if ( container.Parent != null ) {
				NoUnloadRemoveChild( container );
				cursor.Target = null;
			}
			else {
				IsSelecting = !IsSelecting;
			}

			return true;
		}

		return false;
	}

	public bool OnKeyUp ( Key key ) { return true; }

	public bool OnPressed ( PressedEvent @event ) {
		return true;
	}

	public bool OnReleased ( ReleasedEvent @event ) {
		return true;
	}

	public bool OnClicked ( ClickedEvent @event ) {
		if ( @event.Button == CursorButton.Left ) {
			View( cursor.Target );
			AddChild( container, new() { Size = (0, 1000) } );
		}

		IsSelecting = false;
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		if ( !IsSelecting )
			return false;

		if ( root.TriggerCulledEvent( @event, @event.EventPosition, (c, d) => c != this && c.ReceivesPositionalInputAt( d ) ) is UIComponent handler ) {
			cursor.Target = handler;
		}
		else {
			cursor.Target = null;
		}

		return true;
	}
}
