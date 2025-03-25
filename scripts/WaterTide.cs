using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class WaterTide : MeshInstance3D
{
	[Export]
	private MeshInstance3D meshInstance3D;

	[Export]
	public float magnitude = 0.25f;
	[Export]
	public float speed = 1.8f;

	private Vector3 startLocation;

	private float yPosition;
	private float sinePosition;

	public override void _Ready()
	{
		base._Ready();
		startLocation = GlobalPosition;

		yPosition = GlobalPosition.Y;
	}

	public override void _Process(double delta)
	{
		sinePosition += speed * (float)delta;

		yPosition = startLocation.Y + magnitude +  Mathf.Sin(sinePosition)*magnitude;

		GlobalPosition = new Vector3(GlobalPosition.X, yPosition, GlobalPosition.Z);

		StandardMaterial3D standardMaterial3D = (StandardMaterial3D)meshInstance3D.GetSurfaceOverrideMaterial(0);
		
		float waveSpeed = 0.1f;
		standardMaterial3D.Uv1Offset = new Vector3(standardMaterial3D.Uv1Offset.X + waveSpeed * (float)delta, standardMaterial3D.Uv1Offset.Y + (waveSpeed/2f) * (float)delta, 0f);
		
	}
}
