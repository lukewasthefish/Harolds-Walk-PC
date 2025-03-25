using Godot;
using System;

public partial class dashPanel : Node3D
{
	[Export]
	private AudioStream sound;
	public override void _Process(double delta)
	{
		this.Rotate(this.Basis.Y, (float)delta * 8f);
	}

	private void BoostPlayer(Area3D area){
		if(area.IsInGroup("Player"))
		{
			GameManager.Instance.playermovement.Boost(128f, GameManager.Instance.playermovement.Basis.Z);
			GameManager.Instance.soundManager.PlaySound(sound);
		}
	}
}
