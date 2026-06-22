using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.UiPro;

[Title( "Input System - UI Pro" ), Category( "UI Pro" ), Icon( "mouse" )]
public class UiInputSystem : Component
{
	private NodeComponent _lastHovered = null;
	private NodeComponent _lastPressed = null;

	protected override void OnUpdate()
	{
		IEnumerable<UiCanvas> canvases = Scene.GetAllComponents<UiCanvas>();

		if ( !canvases.Any() )
		{
			ClearState();
			return;
		}

		NodeComponent hovered = null;

		foreach ( UiCanvas canvas in canvases )
		{
			IReadOnlyList<NodeComponent> nodes = canvas.TargetableNodes;
			if ( nodes == null ) continue;

			Vector2 mouse = canvas.ScreenToCanvas( Mouse.Position );

			NodeComponent candidate = GetTopMostNodeAtPoint( nodes, mouse );

			if ( candidate != null )
			{
				hovered = candidate;
				break;
			}
		}

		if ( hovered != _lastHovered )
		{
			Notify<IPointerExitHandler>( _lastHovered, h => h.OnPointerExit() );
			Notify<IPointerEnterHandler>( hovered, h => h.OnPointerEnter() );
			_lastHovered = hovered;
		}

		if ( Input.Pressed( "Attack1" ) )
		{
			_lastPressed = hovered;
			Notify<IPointerDownHandler>( _lastPressed, h => h.OnPointerDown() );
		}

		if ( Input.Released( "Attack1" ) )
		{
			Notify<IPointerUpHandler>( _lastPressed, h => h.OnPointerUp() );

			if ( _lastPressed.IsValid() && _lastPressed == hovered )
				Notify<IPointerClickHandler>( _lastPressed, h => h.OnPointerClick() );

			_lastPressed = null;
		}
	}

	private void ClearState()
	{
		Notify<IPointerExitHandler>( _lastHovered, h => h.OnPointerExit() );
		Notify<IPointerUpHandler>( _lastPressed, h => h.OnPointerUp() );

		_lastHovered = null;
		_lastPressed = null;
	}

	private static void Notify<T>( NodeComponent node, Action<T> action ) where T : class
	{
		if ( !node.IsValid() ) return;

		GameObject gameObject = node.GameObject;

		while ( gameObject.IsValid() )
		{
			IEnumerable<T> handlers = gameObject.Components.GetAll<T>( FindMode.EnabledInSelf );

			if ( handlers.Any() )
			{
				foreach ( T handler in handlers )
					action( handler );

				return;
			}

			gameObject = gameObject.Parent;
		}
	}

	private static NodeComponent GetTopMostNodeAtPoint( IReadOnlyList<NodeComponent> nodes, Vector2 point )
	{
		for ( int i = nodes.Count - 1; i >= 0; i-- )
		{
			NodeComponent node = nodes[i];

			if ( node.ContainsPoint( point ) )
				return node;
		}

		return null;
	}
}
