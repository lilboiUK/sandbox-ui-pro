using System;

namespace Sandbox.UiPro;

[Title( "Button - UI Pro" ), Category( "UI Pro" ), Icon( "smart_button" )]
public class Button : Component, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	private bool _hovered;

	[Property] public NodeComponent Target { get; set; }

	[Property] public Color DefaultTint { get; set; } = Color.White;
	[Property] public Color HoveredTint { get; set; } = Color.Gray;
	[Property] public Color PressedTint { get; set; } = Color.Gray.Darken( 0.5f );

	[Property] public bool AffectBorderColor { get; set; } = false;
	[Property, ShowIf( nameof( AffectBorderColor ), true )] public Color DefaultBorderColor { get; set; } = Color.White;
	[Property, ShowIf( nameof( AffectBorderColor ), true )] public Color HoveredBorderColor { get; set; } = Color.Gray;
	[Property, ShowIf( nameof( AffectBorderColor ), true )] public Color PressedBorderColor { get; set; } = Color.Gray.Darken( 0.5f );

	[Property] public Action OnClick { get; set; }

	protected override void OnEnabled()
	{
		_hovered = false;
		ApplyDefault();
	}

	private void ApplyDefault()
	{
		if ( !Target.IsValid() ) return;
		Target.Style.Tint = DefaultTint;
		if ( AffectBorderColor ) Target.Style.BorderColor = DefaultBorderColor;
	}

	private void ApplyHovered()
	{
		if ( !Target.IsValid() ) return;
		Target.Style.Tint = HoveredTint;
		if ( AffectBorderColor ) Target.Style.BorderColor = HoveredBorderColor;
	}

	private void ApplyPressed()
	{
		if ( !Target.IsValid() ) return;
		Target.Style.Tint = PressedTint;
		if ( AffectBorderColor ) Target.Style.BorderColor = PressedBorderColor;
	}

	void IPointerEnterHandler.OnPointerEnter()
	{
		_hovered = true;
		ApplyHovered();
	}

	void IPointerExitHandler.OnPointerExit()
	{
		_hovered = false;
		ApplyDefault();
	}

	void IPointerDownHandler.OnPointerDown()
	{
		ApplyPressed();
	}

	void IPointerUpHandler.OnPointerUp()
	{
		if ( _hovered ) ApplyHovered();
		else ApplyDefault();
	}

	void IPointerClickHandler.OnPointerClick()
	{
		OnClick?.Invoke();
	}
}
