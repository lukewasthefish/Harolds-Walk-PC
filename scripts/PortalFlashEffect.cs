using Godot;
using System;

public partial class PortalFlashEffect : Panel
{
	StyleBoxFlat styleBox;
	public override void _Ready()
	{
		styleBox = (StyleBoxFlat)this.GetThemeStylebox("panel");
		GameManager.Instance.portalFlashEffect = this;
	}

	public override void _Process(double delta)
	{
		//This is how we'll lerp back to normalcy
		Color previousColor = (Color)styleBox.Get("bg_color");
		styleBox.Set("bg_color", new Color(previousColor.R, previousColor.G, previousColor.B, Mathf.Lerp(previousColor.A, 0f, 4f * (float)delta)));
		AddThemeStyleboxOverride("panel", styleBox);
	}

	/// <summary>
	/// Flashes the screen with a color.
	/// </summary>
	/// <param name="color"></param>
	/// <param name="timeToTurnTransparent"></param>
	public void DoFlash(Color color){
		styleBox.Set("bg_color", color);
		AddThemeStyleboxOverride("panel", styleBox);
	}
}
