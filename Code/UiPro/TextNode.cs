namespace Sandbox.UiPro;

public enum HorizontalAlignment
{
	Left,
	Center,
	Right
}

public enum VerticalAlignment
{
	Top,
	Center,
	Bottom
}

[Title( "Text Node - UI Pro" ), Category( "UI Pro" ), Icon( "text_fields" )]
public class TextNode : NodeComponent
{
	[Property, InlineEditor, Group( "Layout Settings" ), Order( -999 )] public override NodeStyle Style { get; set; } = GetDefaultStyle();

	[Property, Group("Text Settings")] public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;
	[Property, Group( "Text Settings" )] public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
	[Property, InlineEditor, Group( "Text Settings" )] public TextRendering.Scope TextScope { get; set; } = TextRendering.Scope.Default;

	private static NodeStyle GetDefaultStyle()
	{
		return new NodeStyle()
		{
			Anchor = NodePoint.CenterMiddle,
			Pivot = NodePoint.CenterMiddle,
			Offset = Vector2.Zero,
			Size = new Vector2( 100, 100 ),
			ChildPadding = Vector2.Zero,
			ClipChildren = false,
			StretchHorizontal = false,
			StretchVertical = false,
			CornerRadius = 0,
			BorderWidth = 0,
			BorderColor = Color.Black,
			Texture = Texture.White,
			Tint = Color.White,
			UvScale = Vector2.One,
			UvOffset = Vector2.Zero
		};
	}

	protected override void UpdateStyle(float scaleFactor)
	{
		Style.BorderWidth = 0; // for debugging
		Style.BorderColor = Color.White;

		TextRendering.Scope scope = TextScope;
		scope.FontSize *= scaleFactor;

		Texture texture = TextRendering.GetOrCreateTexture( scope );
		Style.Texture = texture;

		Vector2 scale = (Layout.Outer.Size * scaleFactor) / texture.Size;
		Style.UvScale = scale;

		float alignX = HorizontalAlignment switch
		{
			HorizontalAlignment.Left => 0f,
			HorizontalAlignment.Center => 0.5f,
			HorizontalAlignment.Right => 1f,
			_ => 0.5f,
		};

		float alignY = VerticalAlignment switch
		{
			VerticalAlignment.Top => 0f,
			VerticalAlignment.Center => 0.5f,
			VerticalAlignment.Bottom => 1f,
			_ => 0.5f,
		};

		float offsetX = alignX * (1f / scale.x - 1f);
		float offsetY = alignY * (1f / scale.y - 1f);

		Style.UvOffset = new Vector2( offsetX, offsetY );
	}
}
