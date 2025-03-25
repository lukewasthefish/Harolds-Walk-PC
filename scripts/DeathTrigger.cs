using Godot;
using System;

public partial class DeathTrigger : Area3D
{
	public override void _Ready()
	{
		base._Ready();

		this.AreaEntered += Death;
	}

	private void Death(Area3D area)
	{
		if(area.IsInGroup("Player") && GameManager.Instance.playermovement.movementEnabled)
		{
			if(Extensions.IsValid(TriggerCube.triggerCubeManager) && TriggerCube.triggerCubeManager.midOperation)
			{
				return;
			}

			GameManager.Instance.playermovement.DeathInstant();
		}
	}
}
