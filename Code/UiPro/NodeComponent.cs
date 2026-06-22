using System;

namespace Sandbox.UiPro;

public abstract class NodeComponent : Component
{
	public static readonly Material Material = Material.FromShader( "shaders/node.shader" );

	[Property, InlineEditor, Group( "Layout Settings" ), Order( -999 )] public virtual NodeStyle Style { get; set; }
	
	[Property] public bool ReceivePointerInput { get; set; } = false;

	public NodeLayout Layout { get; private set; } = new NodeLayout();

	public void Update( NodeLayout parentLayout, float scaleFactor )
	{
		UpdateStyle( scaleFactor );
		Layout.Compute( parentLayout, Style );
	}

	protected virtual void UpdateStyle(float scaleFactor) { }

	public RenderAttributes GetRenderAttributes( Vector2 viewportSize )
	{
		RenderAttributes renderAttributes = new RenderAttributes();

		Vector2 ndcPos = ScreenPositionToNdc( Layout.Outer.Position, viewportSize );
		Vector2 ndcSize = ScreenSizeToNdc( Layout.Outer.Size, viewportSize );

		renderAttributes.Set( "NodeRect", new Vector4( ndcPos.x, ndcPos.y, ndcSize.x, ndcSize.y ) );
		renderAttributes.Set( "NodeScreenRect", new Vector4( Layout.Outer.Position.x, Layout.Outer.Position.y, Layout.Outer.Size.x, Layout.Outer.Size.y ) );
		renderAttributes.Set( "Texture", Style.Texture );
		renderAttributes.Set( "Tint", Style.Tint );
		renderAttributes.Set( "UvScale", Style.UvScale );
		renderAttributes.Set( "UvOffset", Style.UvOffset );
		renderAttributes.Set( "BorderColor", Style.BorderColor );
		renderAttributes.Set( "NodeSize", Layout.Outer.Size );
		renderAttributes.Set( "CornerRadius", Style.CornerRadius );
		renderAttributes.Set( "BorderWidth", Style.BorderWidth );
		renderAttributes.Set( "ClipRect", new Vector4( Layout.ClipRect.Position.x, Layout.ClipRect.Position.y, Layout.ClipRect.Size.x, Layout.ClipRect.Size.y ) );
		renderAttributes.Set( "ClipRadius", Layout.ClipRadius );

		return renderAttributes;
	}

	private static Vector2 ScreenPositionToNdc( Vector2 position, Vector2 viewport )
	{
		float x = position.x / viewport.x * 2.0f - 1.0f;
		float y = 1.0f - position.y / viewport.y * 2.0f;
		return new Vector2( x, y );
	}

	private static Vector2 ScreenSizeToNdc( Vector2 size, Vector2 viewport )
	{
		float x = size.x / viewport.x * 2.0f;
		float y = size.y / viewport.y * 2.0f;
		return new Vector2( x, y );
	}

	public bool ContainsPoint( Vector2 point )
	{
		if ( !IsInsideRoundedRect( Layout.Outer, Style.CornerRadius, point ) )
			return false;

		if ( !IsInsideRoundedRect( Layout.ClipRect, Layout.ClipRadius, point ) )
			return false;

		return true;
	}

	protected static bool IsInsideRoundedRect( Rect rect, float radius, Vector2 point )
	{
		if ( !rect.IsInside( point ) )
			return false;

		if ( radius <= 0f )
			return true;

		float r = Math.Min( radius, Math.Min( rect.Width, rect.Height ) * 0.5f );

		float cx = Math.Clamp( point.x, rect.Left + r, rect.Right - r );
		float cy = Math.Clamp( point.y, rect.Top + r, rect.Bottom - r );

		float dx = point.x - cx;
		float dy = point.y - cy;

		return dx * dx + dy * dy <= r * r;
	}
}
