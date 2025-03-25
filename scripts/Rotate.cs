using Godot;
using System;

/// <summary>
/// Rotates an object on the Roll, Pitch, and Yaw directions by the x, y, and z parameters provided in the vector
/// </summary>
public partial class Rotate : Node3D
{
	[Export]
	//Ammount to rotate in Vector3 form per second
	private Vector3 rotateAmmount = Vector3.Zero;

	public override void _Process(double delta)
	{
		//Roll
		this.Rotate(Vector3.Forward, rotateAmmount.Z * (float)delta);

		//Pitch
		this.Rotate(Vector3.Right, rotateAmmount.X * (float)delta);

		//Yaw
		this.Rotate(Vector3.Up, rotateAmmount.Y * (float)delta);
	}
}
