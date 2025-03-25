using Godot;
using System;

public partial class SinePlatform : Node3D
{
	private float initialY;
	private float sineTime = 0.0f;
	[Export]
	private float timeOffset = 0.0f;

	[Export]
	private float amplitude = 1f;

	[Export]
	private float frequency = 1f;

	public override void _Ready()
	{
		sineTime = timeOffset;
		initialY = this.GlobalPosition.Y;
	}

	public override void _Process(double delta)
	{
		this.GlobalPosition = new Vector3(this.GlobalPosition.X, initialY + (Mathf.Sin(sineTime) * amplitude), this.GlobalPosition.Z);
		sineTime += (float)delta * frequency;
	}
}
