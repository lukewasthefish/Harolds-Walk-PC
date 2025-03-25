using Godot;
using System;

public abstract partial class Enemy : CharacterBody3D
{
	protected bool alert = false;
	protected int alertDistance = 25;
	protected bool grounded = true;

	public abstract void PlayerCollision(playermovement player);
	public abstract void Death(bool acidDeath = false);
}
