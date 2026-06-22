namespace Sandbox;

// Just makes the mouse visible so you can click stuff
public class ExampleMouseEnabler : Component
{
	protected override void OnStart()
	{
		Mouse.Visibility = MouseVisibility.Visible;
	}
}
