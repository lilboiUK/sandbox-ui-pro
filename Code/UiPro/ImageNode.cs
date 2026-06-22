namespace Sandbox.UiPro;

[Title( "Image Node - UI Pro" ), Category( "UI Pro" ), Icon( "image" )]
public class ImageNode : NodeComponent
{
	private string _loadedPath;
	private Texture _texture;

	[Property, InlineEditor, Group( "Layout Settings" ), Order( -999 )] public override NodeStyle Style { get; set; } = GetDefaultStyle();

	[Property, ImageAssetPath, Title( "Image" ), Group( "Image Settings" )] public string ImagePath { get; set; }

	[Property, Group( "Image Settings" )] public Color Tint { get => Style.Tint; set => Style.Tint = value; }
	[Property, Group( "Image Settings" )] public float CornerRadius { get => Style.CornerRadius; set => Style.CornerRadius = value; }
	[Property, Group( "Image Settings" )] public float BorderWidth { get => Style.BorderWidth; set => Style.BorderWidth = value; }
	[Property, Group( "Image Settings" )] public Color BorderColor { get => Style.BorderColor; set => Style.BorderColor = value; }

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
		if ( _loadedPath != ImagePath )
		{
			_texture = string.IsNullOrEmpty( ImagePath ) ? null : Texture.LoadFromFileSystem( ImagePath, FileSystem.Mounted );
			_loadedPath = ImagePath;
		}

		Style.Texture = _texture ?? Texture.White;
	}
}
