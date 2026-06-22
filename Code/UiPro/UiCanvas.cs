using Sandbox.Rendering;
using System;
using System.Collections.Generic;

namespace Sandbox.UiPro;

public enum CanvasScaleMode
{
	ConstantPixelSize,
	ScaleWithScreenSize,
}

// I will add comments to this at some point :.)

[Title( "Canvas - UI Pro" ), Category( "UI Pro" ), Icon( "dashboard" )]
public class UiCanvas : Component
{
	private List<NodeComponent> _nodes;

	// For input system
	public IReadOnlyList<NodeComponent> TargetableNodes => _targetableNodes;
	private List<NodeComponent> _targetableNodes;

	[Property, Group( "Canvas Scaler" )]
	public CanvasScaleMode ScaleMode { get; set; } = CanvasScaleMode.ScaleWithScreenSize;

	[Property, Group( "Canvas Scaler" ), ShowIf( nameof( ScaleMode ), CanvasScaleMode.ScaleWithScreenSize )]
	public Vector2 ReferenceResolution { get; set; } = new Vector2( 1920, 1080 );

	[Property, Range( 0f, 1f ), Group( "Canvas Scaler" ), ShowIf( nameof( ScaleMode ), CanvasScaleMode.ScaleWithScreenSize )]
	public float MatchWidthOrHeight { get; set; } = 0.5f;

	public float ScaleFactor { get; private set; } = 1f;

	private CommandList _commands;
	private GpuBuffer<Vertex> _vertexBuffer;

	private static readonly Vertex[] UnitQuad =
	[
		new( new Vector3( 0f, 0f, 0f ) ),
		new( new Vector3( 1f, 0f, 0f ) ),
		new( new Vector3( 1f, 1f, 0f ) ),

		new( new Vector3( 0f, 0f, 0f ) ),
		new( new Vector3( 1f, 1f, 0f ) ),
		new( new Vector3( 0f, 1f, 0f ) ),
	];

	protected override void OnEnabled()
	{
		_nodes = new List<NodeComponent>();
		_targetableNodes = new List<NodeComponent>();

		_commands = new CommandList();
		Scene.Camera?.AddCommandList( _commands, Stage.AfterPostProcess );

		_vertexBuffer = new GpuBuffer<Vertex>( 6, GpuBuffer.UsageFlags.Vertex );
		_vertexBuffer.SetData( UnitQuad );
	}

	protected override void OnDisabled()
	{
		_nodes = null;
		_targetableNodes = null;

		if ( _commands != null )
		{
			Scene.Camera?.RemoveCommandList( _commands );
			_commands = null;
		}

		_vertexBuffer?.Dispose();
		_vertexBuffer = null;
	}

	protected override void OnUpdate()
	{
		UpdateNodes();
	}

	protected override void OnPreRender()
	{
		_commands.Reset();
		DrawNodes();
	}

	private void UpdateNodes()
	{
		_nodes.Clear();
		_targetableNodes.Clear();

		ScaleFactor = ComputeScaleFactor();

		Vector2 virtualSize = Screen.Size / ScaleFactor;

		foreach ( GameObject child in GameObject.Children )
			UpdateNode( child, NodeLayout.GetRootLayout( virtualSize ) );
	}

	private float ComputeScaleFactor()
	{
		if ( ScaleMode == CanvasScaleMode.ConstantPixelSize )
			return 1f;

		Vector2 screen = Screen.Size;
		Vector2 reference = ReferenceResolution;

		if ( reference.x <= 0f || reference.y <= 0f || screen.x <= 0f || screen.y <= 0f )
			return 1f;

		float logWidth = MathF.Log2( screen.x / reference.x );
		float logHeight = MathF.Log2( screen.y / reference.y );
		float logScale = MathX.Lerp( logWidth, logHeight, MatchWidthOrHeight );

		return MathF.Pow( 2f, logScale );
	}

	private void UpdateNode( GameObject nodeGameObject, NodeLayout parentLayout )
	{
		NodeComponent node = nodeGameObject.GetComponent<NodeComponent>();

		if ( node != null )
		{
			node.Update( parentLayout, ScaleFactor );

			_nodes.Add( node );

			if ( node.ReceivePointerInput )
				_targetableNodes.Add( node );

			foreach ( GameObject child in nodeGameObject.Children )
				UpdateNode( child, node.Layout );
		}
		else
		{
			foreach ( GameObject child in nodeGameObject.Children )
				UpdateNode( child, parentLayout );
		}
	}

	public Vector2 ScreenToCanvas( Vector2 screenPosition )
	{
		float scale = ScaleFactor <= 0f ? 1f : ScaleFactor;
		return screenPosition / scale;
	}

	private void DrawNodes()
	{
		Vector2 virtualViewport = Screen.Size / ScaleFactor;

		foreach ( NodeComponent node in _nodes )
			_commands.Draw( _vertexBuffer, NodeComponent.Material, 0, 6, node.GetRenderAttributes( virtualViewport ), Graphics.PrimitiveType.Triangles );
	}
}
