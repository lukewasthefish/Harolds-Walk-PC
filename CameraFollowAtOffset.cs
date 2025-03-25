using Godot;
using System;

public partial class CameraFollowAtOffset : Node3D
{
	[Export]
	protected Node3D target = null;

	[Export]
	private Vector3 offset = Vector3.Up + Vector3.Back * 8f;

	[Export]
	private bool lookAtTarget = true;

	[Export]
	private bool lerp = true;

	[Export]
	private float lerpspeed = 12f;

	[Export]
	private bool findPlayerAtReady = false;

	public override void _Process(double delta)
	{
		if(findPlayerAtReady && Extensions.IsValid(GameManager.Instance) && Extensions.IsValid(GameManager.Instance.playermovement))
		{
			if(Extensions.IsValid(GameManager.Instance.playermovement))		
				target = GameManager.Instance.playermovement;
		}

		if(!Extensions.IsValid(target))
		{
			return;
		}

		var newTransform = new Transform3D(Transform.Basis, Transform.Origin);

		Vector3 destination = new Vector3(target.Transform.Origin.X + offset.X, target.Transform.Origin.Y + offset.Y, target.Transform.Origin.Z + offset.Z);

		if(lerp)
		{
			newTransform.Origin = newTransform.Origin.Lerp(destination, lerpspeed * (float)delta);
		} else 
		{
			newTransform.Origin = destination;
		}

		if(lookAtTarget)
		{
			newTransform = newTransform.LookingAt(target.Transform.Origin, Vector3.Up);
		}

		Transform = newTransform;
	}
}
