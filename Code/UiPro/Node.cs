using System;

namespace Sandbox.UiPro;

public enum NodePoint
{
	TopLeft, TopMiddle, TopRight,
	CenterLeft, CenterMiddle, CenterRight,
	BottomLeft, BottomMiddle, BottomRight,
}

public class NodeStyle
{
	[Property] public NodePoint Anchor { get; set; }
	[Property] public NodePoint Pivot { get; set; }
	[Property] public Vector2 Offset { get; set; }
	[Property] public Vector2 Size { get; set; }
	[Property] public Vector4 ChildPadding { get; set; }
	[Property] public bool ClipChildren { get; set; }
	[Property] public bool StretchHorizontal { get; set; }
	[Property] public bool StretchVertical { get; set; }
	
	[Property, Hide] public float CornerRadius { get; set; }
	[Property, Hide] public float BorderWidth { get; set; }
	[Property, Hide] public Color BorderColor { get; set; }
	[Property, Hide] public Texture Texture { get; set; }
	[Property, Hide] public Color Tint { get; set; }
	[Property, Hide] public Vector2 UvScale { get; set; }
	[Property, Hide] public Vector2 UvOffset { get; set; }
}

public class NodeLayout
{
	public Rect Outer { get; private set; }
	public Rect Inner { get; private set; }
	public Rect ClipRect { get; private set; }
	public float ClipRadius { get; private set; }
	public Rect ChildClipRect { get; private set; }
	public float ChildClipRadius { get; private set; }

	public static NodeLayout GetRootLayout( Vector2 size )
	{
		Rect rootRect = new Rect( Vector2.Zero, size );

		NodeLayout layout = new NodeLayout()
		{
			Outer = rootRect,
			Inner = rootRect,
			ClipRect = rootRect,
			ClipRadius = 0,
			ChildClipRect = rootRect,
			ChildClipRadius = 0
		};

		return layout;
	}

	public void Compute( NodeLayout parentLayout, NodeStyle style )
	{
		Vector2 anchor = AnchorFraction( style.Anchor );
		Vector2 pivot = AnchorFraction( style.Pivot );

		float width = style.StretchHorizontal ? parentLayout.Inner.Size.x : style.Size.x;
		float height = style.StretchVertical ? parentLayout.Inner.Size.y : style.Size.y;

		float x = style.StretchHorizontal
			? parentLayout.Inner.Position.x + style.Offset.x
			: parentLayout.Inner.Position.x + anchor.x * parentLayout.Inner.Size.x - pivot.x * width + style.Offset.x;

		float y = style.StretchVertical
			? parentLayout.Inner.Position.y + style.Offset.y
			: parentLayout.Inner.Position.y + anchor.y * parentLayout.Inner.Size.y - pivot.y * height + style.Offset.y;

		Outer = new Rect( new Vector2( x, y ), new Vector2( width, height ) );

		float innerW = Math.Max( 0f, width - style.ChildPadding.x - style.ChildPadding.z );
		float innerH = Math.Max( 0f, height - style.ChildPadding.y - style.ChildPadding.w );
		Inner = new Rect( new Vector2( x + style.ChildPadding.x, y + style.ChildPadding.y ), new Vector2( innerW, innerH ) );

		ClipRect = parentLayout.ChildClipRect;
		ClipRadius = parentLayout.ChildClipRadius;

		if ( style.ClipChildren )
		{
			float bw = Math.Max( 0f, style.BorderWidth );
			float clipW = Math.Max( 0f, Outer.Size.x - bw * 2f );
			float clipH = Math.Max( 0f, Outer.Size.y - bw * 2f );

			ChildClipRect = new Rect( new Vector2( Outer.Position.x + bw, Outer.Position.y + bw ), new Vector2( clipW, clipH ) );
			ChildClipRadius = Math.Max( 0f, style.CornerRadius - bw );
		}
		else
		{
			ChildClipRect = parentLayout.ChildClipRect;
			ChildClipRadius = parentLayout.ChildClipRadius;
		}
	}

	private static Vector2 AnchorFraction( NodePoint point )
	{
		float fx = point switch
		{
			NodePoint.TopLeft or NodePoint.CenterLeft or NodePoint.BottomLeft => 0f,
			NodePoint.TopMiddle or NodePoint.CenterMiddle or NodePoint.BottomMiddle => 0.5f,
			_ => 1f,
		};
		float fy = point switch
		{
			NodePoint.TopLeft or NodePoint.TopMiddle or NodePoint.TopRight => 0f,
			NodePoint.CenterLeft or NodePoint.CenterMiddle or NodePoint.CenterRight => 0.5f,
			_ => 1f,
		};
		return new Vector2( fx, fy );
	}
}
