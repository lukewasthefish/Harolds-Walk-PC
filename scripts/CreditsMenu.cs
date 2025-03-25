using Godot;
using System;

public partial class CreditsMenu : Label
{
	public override void _Ready()
	{
		this.Text += "\nMade in Godot, license for the Godot engine is as follows:\n\n" + Engine.GetLicenseText();
	}
}