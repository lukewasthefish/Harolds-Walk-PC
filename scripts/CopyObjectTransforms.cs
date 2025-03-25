using Godot;
using System;

public partial class CopyObjectTransforms : Node3D
{
	[Export]
	private Node3D objectToCopy;

	[Export]
	private bool copyLocation = true;
	[Export]
	private bool copyRotation = true;
	[Export]
	private bool copyScale = true;

	[Export]
	private bool useCameraInstead = false;
	
	public override void _Ready()
	{
		if(useCameraInstead)
		{
			objectToCopy = GameManager.Instance.playermovement.cameraNode;
		}
	}

	public override void _Process(double delta)
	{
		if(!Extensions.IsValid(objectToCopy))
		{
			if(useCameraInstead && Extensions.IsValid(GameManager.Instance.playermovement) && Extensions.IsValid(GameManager.Instance.playermovement.cameraNode))
			{
				objectToCopy = GameManager.Instance.playermovement.cameraNode;
			}

			return;
		}

		if(copyLocation)
		{
			this.GlobalPosition = objectToCopy.GlobalPosition;
		}

		if(copyRotation)
		{
			this.Rotation = objectToCopy.Rotation;
		}

		if(copyScale)
		{
			this.Scale = objectToCopy.Scale;
		}
	}
}
