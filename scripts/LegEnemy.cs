using Godot;
using System;
public partial class LegEnemy : Enemy
{
	[Export]
	private AnimationPlayer animationPlayer;
	private Vector3 targetDirection = Vector3.Zero;

	[Export]
	private CollisionShape3D CollisionShape3D;

	[Export]
	private MeshInstance3D mesh;

	[Export]
	private Material acidicMaterial;

	[Export]
	private GpuParticles3D acidFallParticles;

	[Export]
	private AudioStream deathSound;
	[Export]
	private AudioStream acidDeathSound;

	private const float SPEED = 9f;

	private bool dead = false;

	private void PlayerLandOnHead(Area3D area)
	{
		//Player needs to be falling or rolling.
		//If player is standing still we should not be killed
		if(!dead && area.IsInGroup("Player") && !GameManager.Instance.playermovement.dead)
		{
			if(GameManager.Instance.playermovement.grounded && !GameManager.Instance.playermovement.isRolling && GameManager.Instance.playermovement.moveDirection.Y <= (-playermovement.STICK_TO_GROUND_FORCE) + 10f)
			{
				GameManager.Instance.playermovement.TakeDamage();
				return;
			}

			GameManager.Instance.steamManager.UnlockSteamAchievement("leg_enemy_defeated");
			Death();

			if(!GameManager.Instance.playermovement.isRolling)
				GameManager.Instance.playermovement.moveDirection.Y = 35f;
		}
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if(dead || !Extensions.IsValid(GameManager.Instance.playermovement))
		{
			CollisionShape3D.Disabled = true;
			return;
		}
		Vector3 playerPosition = GameManager.Instance.playermovement.GlobalPosition;

		if(GameManager.Instance.playermovement.dead)
		{
			playerPosition = this.GlobalPosition + Vector3.Forward;
		}

		Transform3D lookAtTransform = this.GlobalTransform.LookingAt(new Vector3(playerPosition.X, this.GlobalPosition.Y, playerPosition.Z), Vector3.Up);
		
		float angleTo = this.Basis.Z.AngleTo(lookAtTransform.Basis.Z);
		float distanceFromPlayer = this.GlobalPosition.DistanceTo(GameManager.Instance.playermovement.GlobalPosition);
		if (!alert)
		{
			animationPlayer.Play("Idle", -1, 1f);
			if (distanceFromPlayer <= alertDistance)
			{
				alert = true;
			}
		}
		else
		{
			Vector3 moveDirection = Vector3.Zero;

			if(angleTo < 3f){
				//Figure out which direction takes us closer to being in line with the pursuit of the player
				int closestDirectionSign = 1;

				//Do two tests and pick the sign that produces the closest angle, i.e should we be turning clockwise or counter-clockwise to better intercept the player?
				this.Rotate(Vector3.Up, 0.1f * (float)delta);

				Transform3D lookAtTransform1 = Transform.LookingAt(new Vector3(playerPosition.X, this.GlobalPosition.Y, playerPosition.Z), Vector3.Up);
				float angleTo1 = this.Basis.Z.AngleTo(lookAtTransform1.Basis.Z);

				//Undo previous test...
				this.Rotate(Vector3.Up, -0.1f * (float)delta);

				//The second test
				this.Rotate(Vector3.Up, -0.1f * (float)delta);

				Transform3D lookAtTransform2 = Transform.LookingAt(new Vector3(playerPosition.X, this.GlobalPosition.Y, playerPosition.Z), Vector3.Up);
				float angleTo2 = this.Basis.Z.AngleTo(lookAtTransform2.Basis.Z);
			
				if(angleTo1 > angleTo2)
				{
					closestDirectionSign = 1;
				} else {
					closestDirectionSign = -1;
				}

				this.Rotate(Vector3.Up, 3f * closestDirectionSign * (float)delta);
			}
			// Move forward
			moveDirection = Transform.Basis.Z.Normalized() * SPEED;

			// Gravity and groundchecking
			var downwardQuery = PhysicsRayQueryParameters3D.Create(this.GlobalPosition + Vector3.Up, this.GlobalPosition + Vector3.Down * 0.1f);

			if (IsOnFloor())
			{
				grounded = true;
				if(moveDirection.Y < 0f)
					moveDirection.Y = 0f;
			}
			else
			{
				grounded = false;
			}

			if(!grounded)
			{
				moveDirection = Vector3.Down * 22f;
			}

			animationPlayer.Play("Run", -1, 1.4f);

			if(distanceFromPlayer >= alertDistance * 2){
				alert = false;
			}

			Velocity = moveDirection;
			MoveAndSlide();
		}
	}

	public override void PlayerCollision(playermovement player)
	{
		if(dead)
		{
			return;
		}

		if((player.GlobalPosition.Y > this.GlobalPosition.Y+1f) || player.isRolling)
		{
			if(!player.isRolling)
				player.moveDirection += player.jumpVelocity * Vector3.Up;
			Death();
		} else {
			player.TakeDamage();
		}
	}

	public override void Death(bool acidDeath = false)
	{
		if(dead)
		{
			return;
		}

		if(acidDeath)
		{
			mesh.SetSurfaceOverrideMaterial(0, acidicMaterial);
			acidFallParticles.Emitting = true;
			GameManager.Instance.soundManager.PlaySound(acidDeathSound);
		} else {
			GameManager.Instance.soundManager.PlaySound(deathSound, 0.2f);
		}

		animationPlayer.Play("Collapse", -1, 1f);
		dead = true;
	}
}
