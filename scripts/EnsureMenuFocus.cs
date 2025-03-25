using Godot;
using System;
using System.Collections.Generic;

public partial class EnsureMenuFocus : Node
{
	[Export]
	private Control defaultFocus;

	private Dictionary<Button, Vector2> buttonInitialScales = new Dictionary<Button, Vector2>();

	[Export]
	private bool doButtonScaling = false;

	public override void _Ready()
	{
		defaultFocus.GrabFocus();

		defaultFocus.VisibilityChanged += VisibilityChanged;
	}

    private void VisibilityChanged()
	{
		defaultFocus.GrabFocus();
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

		if(doButtonScaling)
		{
			ScaleButtons(delta);
		}
    }

	/// <summary>
	/// Visual lerp effect
	/// </summary>
	private void ScaleButtons(double delta)
	{
		Control focus = GetViewport().GuiGetFocusOwner();
		Button button = null;
		if(focus is Button button1)
		{
			button = button1;
			if(!buttonInitialScales.ContainsKey(button))
			{
				buttonInitialScales[button] = button.Scale;
			}

			button.Scale = button.Scale.Lerp(buttonInitialScales[button] * 1.14f, 8f * (float)delta);
			button.PivotOffset = button.Size / 2f;
		}

		foreach(Button b in buttonInitialScales.Keys)
		{
			if(button != null && b != button)
			{
				b.Scale = b.Scale.Lerp(buttonInitialScales[b], 12f * (float)delta);
			}
		}
	}
}
