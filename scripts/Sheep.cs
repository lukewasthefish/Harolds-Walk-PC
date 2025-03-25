using Godot;
using System;

public partial class Sheep : CharacterBody3D
{
	private Vector3 initialScale;
	private Vector3 adjustedScale;
	private Vector3 moveDirection;
	private Vector3 launchDirection;

	private float maxFallSpeed = 32.0f;

	[Export]
	private Node3D model;

	[Export]
	private Area3D hitArea;

    public override void _Ready()
    {
        base._Ready();

		initialScale = model.Scale;

		ChooseNewMoveDirection();
		hitArea.AreaEntered += _on_area_entered;
    }

    public override void _Process(double delta)
	{
		model.GlobalPosition = this.GlobalPosition;
		model.Scale = adjustedScale;
		model.Rotation = this.Rotation;
		model.Rotate(Vector3.Up, 90f);

		adjustedScale = adjustedScale.Lerp(initialScale, 6f * (float)delta);

		moveDirection.X = this.Basis.Z.X * Mathf.Pi;
		moveDirection.Z = this.Basis.Z.Z * Mathf.Pi;

		if(this.IsOnFloor())
		{
			SquashAndStretch(2f);
			moveDirection.Y = (float)GD.RandRange(7.0f, 28.0f);
			launchDirection += this.Basis.Z;
		}

		Velocity = (moveDirection + launchDirection);
		MoveAndSlide();

		//Gravity
		moveDirection.Y -= 48f * (float)delta;

		if(moveDirection.Y < -maxFallSpeed)
		{
			moveDirection.Y = -maxFallSpeed;
		}

		launchDirection = launchDirection.Lerp(Vector3.Zero, 1.4f * (float)delta);
	}

	public void SquashAndStretch(float squashFactor)
	{
		adjustedScale = new Vector3(initialScale.X * squashFactor, initialScale.Y / squashFactor, initialScale.Z * squashFactor);
	}

	private void ChooseNewMoveDirection()
	{
		this.Rotate(Vector3.Up, (float)GD.RandRange(0f, 360.0f));

		GetTree().CreateTimer(GD.RandRange(0.1f, 4f)).Timeout += ChooseNewMoveDirection;
	}

	private void _on_area_entered(Area3D area)
	{
		if(area.IsInGroup("Player"))
		{
			SquashAndStretch(1.5f);
			Vector3 storedRotation = this.Rotation;
			this.LookAt(GameManager.Instance.playermovement.GlobalPosition);

			launchDirection = this.Basis.Z * (GameManager.Instance.playermovement.moveDirection.Length() * 2.0f + 1.0f) + Vector3.Up;

			this.Rotation = storedRotation;
		}
	}
}
