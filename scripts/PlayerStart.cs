using Godot;
using System;

public partial class PlayerStart : Node3D
{
	[Export]
	private int deathOffset = 300;

	public override void _Ready()
	{
		if(deathOffset < 0)
		{
			//Death plane should always exists below the players starting position; never above.
			deathOffset = -deathOffset;
		}

		if(!Extensions.IsValid(GameManager.Instance.playermovement))
		{
			return;
		}

		GameManager.Instance.playermovement.cameraNode.rotateAround = this.RotationDegrees.Y + 215;
		GameManager.Instance.playermovement.GlobalPosition = this.GlobalPosition;

		GameManager.Instance.deathPoint = (int)(GameManager.Instance.playermovement.GlobalPosition.Y - 320);

		GameManager.Instance.playermovement.cameraNode.cameraHeight = -45f;

		GameManager.Instance.playermovement.cameraNode.targetOffset = GameManager.Instance.playermovement.GlobalPosition;
		GameManager.Instance.playermovement.cameraNode.destinationY = GameManager.Instance.playermovement.GlobalPosition.Y;

		GameManager.Instance.playermovement.Rotation = new Vector3(GameManager.Instance.playermovement.Rotation.X, this.Rotation.Y, GameManager.Instance.playermovement.Rotation.Z);

		//Adjust boost vector such that we are boosting in the forward direction of the PlayerStart.
		GameManager.Instance.playermovement.boostAddition = GameManager.Instance.playermovement.boostAddition.Length() * this.Basis.Z;

		GameManager.Instance.deathPoint = (int)GameManager.Instance.playermovement.GlobalPosition.Y - deathOffset;
	}
}
