using Godot;
using System;
using System.Linq.Expressions;
using System.Xml.Schema;

public partial class MapCamera : CameraFollowAtOffset
{
	[Export]
	private Node3D playerCursor;

	[Export]
	private Camera3D camera;

	private bool doZoomIn = false;
	private bool doZoomOut = false;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(Extensions.IsValid(playerCursor))
			playerCursor.Scale = Vector3.One * camera.Size / 10;

		int smallest = 32;
		int largest = 200;

		//Input
		if(Input.IsActionJustPressed("MapZoomIn"))
		{
			doZoomIn = true;
		}

		if(Input.IsActionJustPressed("MapZoomOut"))
		{
			doZoomOut = true;
		}

		if(Input.IsActionJustReleased("MapZoomIn"))
		{
			doZoomIn = false;
		}

		if(Input.IsActionJustReleased("MapZoomOut"))
		{
			doZoomOut = false;
		}

		float zoomSpeed = 100f;
		if(doZoomOut)
		{
			camera.Size += zoomSpeed * (float)delta;
		}

		if (doZoomIn)
		{
			camera.Size -= zoomSpeed * (float)delta;
		}

		camera.Size = Mathf.Clamp(camera.Size, smallest, largest);
	}
}
