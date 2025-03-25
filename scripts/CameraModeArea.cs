using Godot;
using System;

public partial class CameraModeArea : Area3D
{
	[Export]
	private PlayerCamera.CameraMode cameraMode;

	[Export]
	private float distance2D = 14.0f;
	[Export]
	private float distanceUp2D = 2.0f;

	[Export]
	private bool returnToNormalOnPlayerExit = true;

	private void UpdateCameraMode(Area3D area)
	{
		if(area.IsInGroup("Player")){
			GameManager.Instance.playermovement.cameraNode.cameraMode = cameraMode;
			GameManager.Instance.playermovement.cameraNode.distance2D = distance2D;
			GameManager.Instance.playermovement.cameraNode.distanceUp2D = distanceUp2D;
		}
	}


	private void ResetCameraMode(Area3D area)
	{
		if(returnToNormalOnPlayerExit && area.IsInGroup("Player")){
			GameManager.Instance.playermovement.cameraNode.cameraMode = PlayerCamera.CameraMode.NORMAL;
			GameManager.Instance.playermovement.cameraNode.SnapAdjustCamera();
		}
	}
}
