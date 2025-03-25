using Godot;
using System;

public partial class Acid : Area3D
{
	private void Death(Area3D area)
	{
		if(area.IsInGroup("Player"))
		{
			GameManager.Instance.playermovement.DeathInstant();
		} else if (area.IsInGroup("Enemy"))
		{
			Enemy enemy = (Enemy)area.GetParent();
			enemy.Death(true);
		}
	}
}
