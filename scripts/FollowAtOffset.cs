using Godot;
using System;

/// <summary>
/// Sets location to target + a specified offset. See the map camera for implementation example
/// </summary>
public partial class FollowAtOffest : Node3D
{
	[Export]
	private Node3D target;
	[Export]
	private Vector3 offset;

	public override void _Process(double delta)
	{
		this.GlobalPosition = target.GlobalPosition + offset;
	}
}
