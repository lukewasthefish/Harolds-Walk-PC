using Godot;
using System;

public partial class LevelSelectIconCamera : Camera3D
{
	public override void _Process(double delta)
	{
		this.GlobalPosition = GameManager.Instance.playermovement.cameraNode.GlobalPosition;
		this.Rotation = GameManager.Instance.playermovement.cameraNode.Rotation;
	}
}
