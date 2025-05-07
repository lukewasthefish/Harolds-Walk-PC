using Godot;
using System;

public partial class PlayerCamera : Camera3D
{
	private Vector3 offsetFromPlayer = Vector3.Back + Vector3.Up;
	
	private float maxHeight = 60f;

	private float minHeight = -120f;

	private float distanceUp = 1f;

	public float rotateAround;

	private const float rotateAxisSpeed = 50f;

	private float initialRotateAxisSpeed;

	public float cameraHeight;

	private float currentDistanceAway;

	private readonly float targetDistance = 3.14f;

	private playermovement playermovement;

	public float destinationY;

	public Vector3 targetOffset;

	public enum CameraMode{NORMAL, SIDE, DISABLED}

	public CameraMode cameraMode = CameraMode.NORMAL;

	//forward vector of camera assuming pitch is removed
	public Vector3 flattendForward;

	private Vector3 lookPosition;
	private Vector3 destinationLookPosition;

	private Vector3 destinationLocation;

	public float distance2D;
	public float distanceUp2D;

	private Vector3 storedRotationOfCamera;

	private float lerpSpeed2D = 0.0f;
	private float lerpSpeed3D = Mathf.Inf;
	private float dest3Dlerpspeed = 100f;

	private Vector2 mouseMovement = Vector2.Zero;
	private bool mouseMovedThisFrame = false;

	public override void _Ready()
	{
		initialRotateAxisSpeed = rotateAxisSpeed;
	}

	public void GoBehindPlayer()
	{
		if(!Extensions.IsValid(GameManager.Instance.playermovement))
		{
			return;
		}

		cameraHeight = -45f;
		rotateAround = 0f;


		targetOffset = playermovement.GlobalPosition;
		destinationY = playermovement.GlobalPosition.Y;
	}

	public void SnapAdjustCamera(float cameraHeight = -45f, float rotateAround = 120f){
		if(!Extensions.IsValid(GameManager.Instance.playermovement))
		{
			return;
		}

		this.cameraHeight = cameraHeight;
		this.rotateAround = rotateAround;

		targetOffset = playermovement.GlobalPosition;
		destinationY = playermovement.GlobalPosition.Y;
	}

	public override void _Process(double delta)
	{
		if(playermovement == null){
			playermovement = GameManager.Instance.playermovement;
		}

		switch(cameraMode)
		{
			case CameraMode.NORMAL:

			HandleInput((float)delta);
			bool transitionFrom2DOver = lerpSpeed3D >= dest3Dlerpspeed - 1.0f;

			//Vertical camera movement
			if (cameraHeight < maxHeight)
			{
				cameraHeight += rotateAxisSpeed * (float)delta;
			}

			if (cameraHeight > minHeight)
			{
				cameraHeight -= rotateAxisSpeed * (float)delta;
			}

			//Adjust camera height if exceeding desired range
			while (cameraHeight < minHeight)
			{
				cameraHeight += initialRotateAxisSpeed * (float)delta;
			}

			while (cameraHeight > maxHeight)
			{
				cameraHeight -= initialRotateAxisSpeed * (float)delta;
			}

			currentDistanceAway = Mathf.Clamp(currentDistanceAway, targetDistance, targetDistance);

			bool aboveDoubleJumpHeight = playermovement.playerModel.GlobalPosition.DistanceTo(playermovement.shadow.GlobalPosition) >= 6.5f;

			if(!aboveDoubleJumpHeight)
			{
				targetOffset = targetOffset.Lerp(new Vector3(playermovement.shadow.Transform.Origin.X, playermovement.shadow.Transform.Origin.Y + distanceUp, playermovement.shadow.Transform.Origin.Z), 4f * (float)delta);
			} else {
				targetOffset = targetOffset.Lerp(new Vector3(playermovement.Transform.Origin.X, playermovement.Transform.Origin.Y, playermovement.Transform.Origin.Z), 3f * (float)delta);
			}

			targetOffset.X = playermovement.playerModel.GlobalPosition.X;
			targetOffset.Z = playermovement.playerModel.GlobalPosition.Z;

			Basis rotation = new Basis(Vector3.Right, Vector3.Up, Vector3.Forward);

			//order matters
			rotation = rotation.Rotated(Vector3.Right, Mathf.DegToRad(cameraHeight));
			rotation = rotation.Rotated(Vector3.Up, Mathf.DegToRad(rotateAround));

			Vector3 vectorMask = Vector3.One;
			Vector3 rotateVector = rotation * vectorMask;

			Vector3 currentCamPosition = targetOffset + Vector3.Up * distanceUp - rotateVector * currentDistanceAway;
			currentCamPosition += Vector3.Up * distanceUp;
			currentCamPosition -= rotateVector * targetDistance;

			currentCamPosition = occludeRay(ref targetOffset, currentCamPosition);

			if (rotateAround > 360)
			{
				rotateAround -= 360f;
			}
			else if (rotateAround < 0f)
			{
				rotateAround += 360f;
			}

			Transform3D newTransform = new Transform3D(Basis.Identity, currentCamPosition);
			if(!aboveDoubleJumpHeight)
			{
				destinationY = Mathf.Lerp(destinationY, playermovement.shadow.GlobalPosition.Y + distanceUp, 4f * (float)delta);
			} else
			{
				destinationY = Mathf.Lerp(destinationY, playermovement.playerModel.GlobalPosition.Y, 3f * (float)delta);
			}

			lookPosition = new Vector3(playermovement.playerModel.GlobalPosition.X, destinationY, playermovement.playerModel.GlobalPosition.Z);

			this.Transform = newTransform;
			this.LookAt(lookPosition);


			float storedPitch = this.Rotation.X;
			this.Rotate(this.Basis.X, -storedPitch);

			flattendForward = this.GlobalTransform.Basis.Z;

			this.Rotate(this.Basis.X, storedPitch);
			lerpSpeed3D = Mathf.Lerp(lerpSpeed3D, dest3Dlerpspeed, 10f * (float)delta);
			lerpSpeed2D = 0.1f;
			break;

			case CameraMode.SIDE:
			lerpSpeed3D = 0.1f;
			storedRotationOfCamera = this.Rotation;
			lerpSpeed2D = Mathf.Lerp(lerpSpeed2D, 7f, 10f * (float)delta);
			this.GlobalPosition = this.GlobalPosition.Lerp(playermovement.GlobalPosition + Vector3.Right * distance2D + Vector3.Up * distanceUp2D, lerpSpeed2D * (float)delta);
			destinationLookPosition = this.GlobalPosition - Vector3.Right;
			this.LookAt(this.GlobalPosition - Vector3.Right);
			Vector3 destinationRotation = this.Rotation;

			this.Rotation = storedRotationOfCamera;

			this.Rotation = this.Rotation.Lerp(destinationRotation, 4f * (float)delta);

			flattendForward = this.GlobalTransform.Basis.Z;
			break;

			case CameraMode.DISABLED:
			//Disabled, do nothing!
			break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			mouseMovement = inputEventMouseMotion.ScreenRelative;
			mouseMovedThisFrame = true;
		}
	}

	private void HandleInput(float delta)
	{
		Vector2 lookVector = Input.GetVector("look_left", "look_right", "look_up", "look_down");

		if (!mouseMovedThisFrame)
		{
			mouseMovement = Vector2.Zero;
		}

		mouseMovedThisFrame = false;

		float sensitivity = 1.4f / (float)GameManager.Instance.optionsManager.options["cameraRotateSpeedScale"];

		float divisionAmmount = 100f * sensitivity;

		float horizontalaxis = 0.0f;
		float verticalaxis = 0.0f;

		if(lookVector.Length() > 0.1f)
		{
			//TODO : Allow configuration of whether or not this is inverted on either axis
			horizontalaxis = -lookVector.X * sensitivity * 2f;
			verticalaxis = -lookVector.Y * sensitivity * 2f;	
		}
		else if (mouseMovement.Length() > 0.01f)
		{
			horizontalaxis = -mouseMovement.X * (sensitivity / Mathf.Pi);
			verticalaxis = -mouseMovement.Y * (sensitivity / Mathf.Pi);
		}

		float maxVelocity = 30f;
		horizontalaxis = Mathf.Clamp(horizontalaxis, -maxVelocity, maxVelocity);
		verticalaxis = Mathf.Clamp(verticalaxis, -maxVelocity, maxVelocity);

		rotateAround += rotateAxisSpeed * horizontalaxis * delta;

		cameraHeight += rotateAxisSpeed * verticalaxis * delta;
	}

	private Vector3 occludeRay(ref Vector3 targetFollow, Vector3 camPosition)
	{
		//We don't want to use any camera collision in the 2D sections of levels.
		if(this.cameraMode == CameraMode.SIDE)
		{
			return camPosition;
		}

		Vector3 origin = targetFollow + Vector3.Up*2;
		Vector3 endpoint = camPosition + (this.Basis.Z/8f);
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		var query = PhysicsRayQueryParameters3D.Create(origin, endpoint);
		Godot.Collections.Dictionary ray = spaceState.IntersectRay(query);
 
		float normalammount = 0.11f;
		if(ray.Count > 0)
		{
			Node collider = (Node)ray["collider"];
			if(collider.IsInGroup("Entity") || collider.IsInGroup("IgnoreCameraCollision")){
				return camPosition;
			}

			Vector3 normal = (Vector3)ray["normal"];
			Vector3 hitposition = (Vector3)ray["position"] + normal * normalammount;
			camPosition = hitposition;
		}

		return camPosition;
	}

	public void SetCameraBehindPlayer(){
		GameManager.Instance.playermovement.cameraNode.rotateAround = this.RotationDegrees.Y + 215;
		GameManager.Instance.playermovement.GlobalPosition = this.GlobalPosition;
		GameManager.Instance.deathPoint = (int)(GameManager.Instance.playermovement.GlobalPosition.Y - 320);

		GameManager.Instance.playermovement.cameraNode.cameraHeight = -45f;

		GameManager.Instance.playermovement.cameraNode.targetOffset = GameManager.Instance.playermovement.GlobalPosition;
		GameManager.Instance.playermovement.cameraNode.destinationY = GameManager.Instance.playermovement.GlobalPosition.Y;

		GameManager.Instance.playermovement.Rotation= new Vector3(GameManager.Instance.playermovement.Rotation.X, this.Rotation.Y, GameManager.Instance.playermovement.Rotation.Z);
	}
}
