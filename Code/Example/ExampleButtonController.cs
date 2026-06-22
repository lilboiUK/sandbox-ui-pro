using Sandbox.UiPro;

namespace Sandbox;

// Hooks up the Button's OnClick event and responds
// by updating the TextNode
public class ExampleButtonController : Component
{
	[Property] public Button MyButton { get; set; }
	[Property] public TextNode MyText { get; set; }
	[Property, ReadOnly] public int ClickCount { get; set; } = 0;

	protected override void OnStart()
	{
		if ( !MyButton.IsValid() ) return;

		MyButton.OnClick = OnButtonClicked;
	}

	private void OnButtonClicked()
	{
		ClickCount++;

		if ( !MyText.IsValid() ) return;

		TextRendering.Scope scope = MyText.TextScope;
		scope.Text = $"Clicked {ClickCount} Times";
		MyText.TextScope = scope;
	}
}
