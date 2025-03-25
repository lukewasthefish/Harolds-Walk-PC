using Godot;
using System;

public partial class TriggerCube : Node3D
{
	[Export]
	private Node3D operationNode;

	[Export]
	private Vector3 alternateRotation = new Vector3(-90.0f, 0.0f, 0.0f);

	[Export]
	private StandardMaterial3D triggerCubeMaterial;

	[Export]
	private AudioStream sound;

	private Vector3 originalRotation;
	private Vector3 destinationRotation = Vector3.Zero;

	private bool useOriginalRotation = true;
	public bool midOperation = false;

	public static TriggerCube triggerCubeManager;

	private Node3D recentTriggerCollision;

	private PlayerCamera.CameraMode recentCameraMode;

	public override void _EnterTree()
	{
		base._EnterTree();

		triggerCubeManager ??= this;
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		triggerCubeManager = null;	
	}

	public override void _Ready()
	{
		base._Ready();

		destinationRotation = operationNode.Rotation;
		originalRotation = operationNode.Rotation;

		alternateRotation.X = Mathf.DegToRad(alternateRotation.X);
		alternateRotation.Y = Mathf.DegToRad(alternateRotation.Y);
		alternateRotation.Z = Mathf.DegToRad(alternateRotation.Z);
	}

	private Color Pink()
	{
		return new Color(255f, 0f, 255f, 1f);
	}

	private Color Green()
	{
		return new Color(0f, 255f, 0f, 1f);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		//All trigger cubes should adjust their color according to the current trigger status.
		if(triggerCubeManager.useOriginalRotation)
		{
			this.triggerCubeMaterial.AlbedoColor = Pink();
		} else 
		{
			this.triggerCubeMaterial.AlbedoColor = Green();
		}

		if(this != triggerCubeManager || !triggerCubeManager.midOperation)
		{
			return;
		}

		//GD.Print(operationNode.Rotation.X + " | " + destinationRotation.X);
		if(operationNode.Rotation.X < destinationRotation.X)
		{
			operationNode.Rotate(Vector3.Right, 1f * (float)delta);
		} else if(operationNode.Rotation.X > destinationRotation.X)
		{
			operationNode.Rotate(Vector3.Right, -1f * (float)delta);
		}

		//if(operationNode.Rotation.Z < destinationRotation.Z)
		//{
			//operationNode.Rotate(Vector3.Forward, 1f * (float)delta);
		//} else if(operationNode.Rotation.Z > destinationRotation.Z)
		//{
			//operationNode.Rotate(Vector3.Forward, -1f * (float)delta);
		//}

		GameManager.Instance.playermovement.GlobalPosition = recentTriggerCollision.GlobalPosition;
		GameManager.Instance.playermovement.LookAt(GameManager.Instance.playermovement.GlobalPosition + Vector3.Right);

		GameManager.Instance.playermovement.moveDirection = Vector3.Zero;

		GameManager.Instance.playermovement.cameraNode.GlobalPosition = GameManager.Instance.playermovement.GlobalPosition + (Vector3.Up) + (recentTriggerCollision.Basis.X * 16.0f);
		GameManager.Instance.playermovement.cameraNode.LookAt(recentTriggerCollision.GlobalPosition);

		GameManager.Instance.playermovement.boostAddition = Vector3.Zero;
		GameManager.Instance.playermovement.speedParticles.Emitting = false;

		if(CheckAlignedWithDirection())
		{
			FinishOperation();
		}
	}

	private void FinishOperation()
	{
		operationNode.Rotation = destinationRotation;
		GameManager.Instance.playermovement.cameraNode.cameraMode = recentCameraMode;
		GameManager.Instance.playermovement.movementEnabled = true;
		GameManager.Instance.playermovement.cameraNode.SnapAdjustCamera();

		Color finalFlashColor = !useOriginalRotation ? Green() : Pink();
		GameManager.Instance.portalFlashEffect.DoFlash(finalFlashColor);

		GetTree().CreateTimer(0.1f).Timeout += DelayAdjustPlayer;

	}

	private void DelayAdjustPlayer()
	{
		GameManager.Instance.playermovement.LookAt(GameManager.Instance.playermovement.GlobalPosition + Vector3.Right);
		GameManager.Instance.playermovement.cameraNode.GlobalPosition = GameManager.Instance.playermovement.GlobalPosition + (Vector3.Up) + (recentTriggerCollision.Basis.X * 16.0f);
		GameManager.Instance.playermovement.moveDirection = Vector3.Zero;
		GameManager.Instance.playermovement.cameraNode.LookAt(recentTriggerCollision.GlobalPosition);
		GameManager.Instance.playermovement.GlobalPosition = recentTriggerCollision.GlobalPosition;

		triggerCubeManager.midOperation = false;
	}

	private bool CheckAlignedWithDirection()
	{
		return Mathf.Abs(destinationRotation.X - operationNode.Rotation.X) < 0.05f;
	}

	private void Trigger(Area3D area3D)
	{
		if(!triggerCubeManager.midOperation && area3D.IsInGroup("Player"))
		{
			triggerCubeManager.midOperation = true;
			triggerCubeManager.recentCameraMode = GameManager.Instance.playermovement.cameraNode.cameraMode;
			triggerCubeManager.recentTriggerCollision = this;
			triggerCubeManager.DoOperation();
		}
	}

	private void DoOperation()
	{
		useOriginalRotation = !useOriginalRotation;

		GameManager.Instance.soundManager.PlaySound(sound);
		GameManager.Instance.portalFlashEffect.DoFlash(useOriginalRotation ? Pink() : Green());

		if(useOriginalRotation)
		{
			destinationRotation = originalRotation;
		} else 
		{
			destinationRotation = alternateRotation;
		}

		GameManager.Instance.playermovement.cameraNode.cameraMode = PlayerCamera.CameraMode.DISABLED;
		GameManager.Instance.playermovement.movementEnabled = false;
	}
}
