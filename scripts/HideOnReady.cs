using Godot;
using System;

public partial class HideOnReady : Node3D
{
	public override void _Ready()
	{
		this.Visible = false;
	}
}
