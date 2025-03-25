using Godot;
using System;

public partial class LookAt : Node3D
{
	[Export]
	private Node3D target;

	[Export]
	private bool usePlayerInsteadOfTarget = false;

    public override void _Ready()
    {
        base._Ready();

		if(usePlayerInsteadOfTarget)
		{
			target = GameManager.Instance.playermovement;
		}
    }

    public override void _Process(double delta)
	{
		this.LookAt(target.GlobalPosition);
	}
}
