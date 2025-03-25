using Godot;
using System;

public partial class Trampoline : Area3D
{
	[Export]
	private float force;

	[Export]
	private Node3D model;

	[Export]
	private AudioStream sound;

	private Vector3 initialBasisX;
	private Vector3 initialBasisY;
	private Vector3 initialBasisZ;

	private const float SQUISH_FACTOR = 1.4f;

	public override void _Ready()
	{
		initialBasisX = this.Transform.Basis.X;
		initialBasisY = this.Transform.Basis.Y;
		initialBasisZ = this.Transform.Basis.Z;
	}

	private void _on_area_entered(Area3D area)
	{
		if(area.IsInGroup("Player"))
		{
			playermovement player = (playermovement)area.GetParent();

			if(Extensions.IsValid(player))
			{
				var newTransform = model.Transform;

				newTransform.Basis.X = initialBasisX * SQUISH_FACTOR;
				newTransform.Basis.Z = initialBasisZ * SQUISH_FACTOR;

				newTransform.Basis.Y = initialBasisY / SQUISH_FACTOR;

				player.moveDirection.Y = force;
				player.canDoDoubleJumpInput = true;
				player.hasAttackedBeforeHittingGround = false;
				player.timerToRollAgain = 0.0f;

				player.gravityScale = playermovement.GRAVITY_SCALE_UPWARDS_JUMP;
				player.grounded = false;

				player.SquashAndStretch(3f);

				model.Transform = newTransform;

				GameManager.Instance.soundManager.PlaySound(sound);
			}
		}
	}

	public override void _Process(double delta)
	{
		//Lerp shape back to initial

		var newTransform = model.Transform;

		newTransform.Basis.X = newTransform.Basis.X.Lerp(initialBasisX, 4f * (float)delta);
		newTransform.Basis.Y = newTransform.Basis.Y.Lerp(initialBasisY, 4f * (float)delta);
		newTransform.Basis.Z = newTransform.Basis.Z.Lerp(initialBasisZ, 4f * (float)delta);

		model.Transform = newTransform;
	}
}
