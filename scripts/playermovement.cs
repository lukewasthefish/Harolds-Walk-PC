using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class playermovement : CharacterBody3D
{
	[Export]
	public PlayerCamera cameraNode = null;
	[Export]
	public Node3D shadow = null;
	[Export]
	public GpuParticles3D speedParticles;
	[Export]
	private GpuParticles3D superJumpParticles;
	[Export]
	private CollisionShape3D collisionShape3D;

	public bool grounded = false;
	public bool isFalling = false;
	
	public bool canDoDoubleJumpInput = true;
	private bool jumpButton;
	private bool rollButton;
	public bool isRolling = false;
	private bool analogGreaterThan0 = false;
	private bool walking = false;
	public bool hasAttackedBeforeHittingGround = true;
	private bool temporaryInvincibility = false;
	private bool landedThisFrame = false;
	private bool leftLedgeThisFrame = false;
	public bool isDoubleJumping = false;
	public bool dead = false;

	private float previousFrameYPosition;
	private float maxMoveSpeedWithoutRolling = 9f;
	private float acceleration = 20f;
	public float jumpVelocity = 28f;
	private float maxFallSpeed = 111f;
	private float rotateSpeed = 11f;
	private float debugGrounded = 0f;
	private float decelleration = 300f;
	private float trueMaxSpeed = 28f;
	private float moveSpeed;
	private float verticalaxis;
	private float horizontalaxis;
	public float gravityScale = GRAVITY_SCALE_UPWARDS_JUMP;
	private float initialMaxMoveSpeed;
	private float forwardLaunch;
	public float timeSinceBoost;
	private float originalCharacterControllerHeight;
	private float leftOverRollSpeed = 10f; //This adds some to your current max speed every time a roll happens. (for Harold Reborn)
	public const float STICK_TO_GROUND_FORCE = 50f;
	public float timeSinceLastGrounded = 0f;
	public float timerToRollAgain = 0f;
	private float RUNSPEED;
	private float WALKSPEED;

	private const float GRAVITY = -9.8f; //An acceleration value representing the gravity 
	public const float GRAVITY_SCALE_UPWARDS_JUMP = 11f;
	public const float GRAVITY_SCALE_FALLING = 19f;
	private const float MAXSLOPEVECTOR = 600f;
	private const float COYOTE_TIME = 0.18f;
	
	public Vector3 moveDirection = new Vector3();
	private Vector3 slopePushVector;
	public Vector3 boostAddition;
	private Vector3 verticalMoveDirection;
	private Vector3 horizontalMoveDirection;
	private Vector3 initialPlayerModelScale;
	private Vector3 adjustedPlayerModelScale; //Used for coded scaling animations

	[Export]
	private AnimationPlayer haroldAnimationPlayer;

	[Export]
	private AnimationPlayer automatonLungAnimationPlayer;

	[Export]
	public Node3D playerModel;

	private Timer disableJumpTimer;

	private float timeSinceLastJump = Mathf.Inf;

	[Export]
	private AudioStreamPlayer playerSoundPlayer;
	[Export]
	private AudioStream[] grunts;
	[Export]
	private AudioStream[] chomps;
	[Export]
	private AudioStream deathSound;
	[Export]
	private AudioStream rollSound;
	[Export]
	private AudioStream sparkleSound;

	public bool movementEnabled = true;

	private bool stairBump = false;

	[Export]
	private Node3D haroldVisuals;
	[Export]
	private Node3D umaVisuals;

	public override void _Ready()
	{
		initialMaxMoveSpeed = maxMoveSpeedWithoutRolling;

		RUNSPEED = initialMaxMoveSpeed * 1.6f;
		WALKSPEED = initialMaxMoveSpeed / 1.5f;

		disableJumpTimer = new Timer();

		initialPlayerModelScale = playerModel.Basis.Scale;

		GameManager.Instance.playermovement = this;

		this.CollisionPriority = 2.0f;
	}

	public override void _Process(double delta)
	{
		base._PhysicsProcess(delta);

		if(!movementEnabled)
		{
			return;
		}

		//GD.Print(this.RotationDegrees.Y + " | " + cameraNode.rotateAround);

		if(this.GlobalPosition.Y < GameManager.Instance.deathPoint)
		{
			DeathInstant();
		}

		this.collisionShape3D.Disabled = dead;

		float deltafloat = (float)delta;
		HandleInput();
		
		bool canJump = false;
		
		if(timerToRollAgain <= 0.3f){
			isRolling = false;
		}

		analogGreaterThan0 = Mathf.Abs(verticalaxis) + Mathf.Abs(horizontalaxis) > 0f;

		//Tests if the player speed is less than top speed, and input is happening through either axis of the joystick.
		if (moveSpeed <= (maxMoveSpeedWithoutRolling + leftOverRollSpeed) && analogGreaterThan0)
		{
			moveSpeed += acceleration * deltafloat;
		}

		if (!analogGreaterThan0)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, 0f, 7f * deltafloat);

			if(moveSpeed < 0.05f)
			{
				moveSpeed = 0f;
			}
		}
		
		if (moveSpeed < (maxMoveSpeedWithoutRolling / 1.5f))
		{
			leftOverRollSpeed = 0f;
		}
		
		float yStore = moveDirection.Y;
		
		isFalling = moveDirection.Y < -3f && !grounded;
		
		if (walking && moveSpeed > maxMoveSpeedWithoutRolling)
		{
			// GD.Print("moveSpeed set to " + maxMoveSpeedWithoutRolling.ToString() + " for exceeding that value.");
			moveSpeed = maxMoveSpeedWithoutRolling;
		}


		float horizontalDivisionFactor = slopePushVector.Length() / 2f;
		// GD.Print("Vertical axis : " + verticalaxis.ToString() + " moveSpeed : " + moveSpeed.ToString());
		if(horizontalDivisionFactor < 1f)
		{
			horizontalDivisionFactor = 1f;
		}
		
		Vector3 destinationVerticalMoveDirection;
		Vector3 destinationHorizontalMoveDirection;


		PlayerCamera pc = GameManager.Instance.playermovement.cameraNode;
		//This will be the flattened forward direction of the camera. A directional input of 'up' should move the player where the camera is pointed.
		if(cameraNode != null)
		{
			destinationVerticalMoveDirection = pc.flattendForward * verticalaxis * moveSpeed;
			destinationHorizontalMoveDirection = (pc.Basis.X * horizontalaxis * moveSpeed) / horizontalDivisionFactor;
		} else 
		{
			destinationVerticalMoveDirection = Transform.Basis.Z * verticalaxis * moveSpeed;
			//Move on an axis perpendicular to the current flattened camera look axis.
			destinationHorizontalMoveDirection = (Transform.Basis.X * horizontalaxis * moveSpeed) / horizontalDivisionFactor;
		}


		verticalMoveDirection = verticalMoveDirection.Lerp(destinationVerticalMoveDirection, 8f * deltafloat);
		horizontalMoveDirection = horizontalMoveDirection.Lerp(destinationHorizontalMoveDirection, 8f * deltafloat);

		if((bool)GameManager.Instance.optionsManager.options["cameraRotateWithPlayer"])
			pc.rotateAround -= horizontalaxis/1.4f * 144.0f * (float)delta;

		if (walking) //Disable acceleration + decelleration when in walking mode
		{
			verticalMoveDirection = destinationVerticalMoveDirection;
			horizontalMoveDirection = destinationHorizontalMoveDirection;
		}

		//This is where moveDirection is 'initialized' for the Process loop.
		//The only reason we don't do it earlier is because we are storing and retrieving the Y component
		moveDirection = (verticalMoveDirection + horizontalMoveDirection) /*+ (jumpOffWallAddition)*/;

		moveDirection = moveDirection.Normalized() * moveSpeed;

		moveDirection += Transform.Basis.Z * forwardLaunch;
		moveDirection += boostAddition;

		moveDirection.Y = yStore;

		if (moveDirection.Y <= 0f && jumpButton)
		{
			if (grounded)
			{
				canJump = true;
			} else if (timeSinceLastGrounded > COYOTE_TIME)
			{
				// anim.SetBool("isJumping", true);
				gravityScale = GRAVITY_SCALE_FALLING;

				if (canDoDoubleJumpInput)
				{
					GruntSound();
					// anim.SetBool("isDoubleJumping", true);
					timeSinceLastJump = 0.0f;
					isDoubleJumping = true;
					canDoDoubleJumpInput = false;
					moveDirection.Y = jumpVelocity * 1.4f;
				}
			}
		}

		timeSinceLastGrounded += deltafloat;

	   	if(timeSinceLastGrounded < COYOTE_TIME)
		{
			canJump = true;
		}

		if(jumpButton && canJump && !dead)
		{
			Jump();
		}

		if(rollButton && !dead)
		{
			Roll(deltafloat);
		}

		if (moveDirection.Y > 0f && !canDoDoubleJumpInput)
		{
			// anim.SetBool("isDoubleJumping", true);
			isDoubleJumping = true;
			gravityScale = GRAVITY_SCALE_UPWARDS_JUMP;
		}

		moveDirection.Y += GRAVITY * Mathf.Abs(gravityScale) * deltafloat;

		if (!grounded)
		{
			moveDirection += Transform.Basis.Y * 32f * (forwardLaunch / 8f) * deltafloat;
		}

		if (analogGreaterThan0 && !dead)
		{
			//Vector3.Up here just ensures that the player is not oriented towards the ground during movement
			Vector3 flatMoveDirection = Transform.Origin - Vector3.Up - new Vector3(moveDirection.X, 0f, moveDirection.Z).Normalized();

			Vector3 lookAtPosition = flatMoveDirection + Vector3.Up;

			if(lookAtPosition != Vector3.Zero)
			Transform = Transform.LookingAt(lookAtPosition, Vector3.Up);
		}

		if (forwardLaunch >= 0) forwardLaunch -= decelleration * deltafloat;
		if (forwardLaunch < 0) forwardLaunch = 0;

		timeSinceBoost += (float)delta;
		timeSinceBoost = Mathf.Clamp(timeSinceBoost, 1f, 1000f);

		Vector3 boostCurveAddition = horizontalaxis * 60.0f * (float)delta * cameraNode.Basis.X * 2.4f / timeSinceBoost;

		if(timeSinceBoost >= 2.0f)
		{
			boostCurveAddition = Vector3.Zero;
		}

		boostAddition += boostCurveAddition;

		boostAddition = boostAddition.Lerp(Vector3.Zero, 1.4f * (float)delta);
		if (Mathf.Abs(boostAddition.Length()) <= 2f) boostAddition = Vector3.Zero;
		//prevents moveSpeed from falling under 0
		if (moveSpeed < 0f)
		{
			moveSpeed = 0f;
		}

		if (moveDirection.Y < -maxFallSpeed)
		{
			moveDirection.Y = -maxFallSpeed;
		}

		if (grounded)
		{
			timeSinceLastGrounded = 0.0f;
			hasAttackedBeforeHittingGround = false;
		}

		if (landedThisFrame)
		{
			SquashAndStretch(1.4f);
		}

		if ((grounded && (forwardLaunch <= 0f)) /*|| (!grounded && moveDirection.y < 0f)*/ || (!grounded && forwardLaunch < 4f && moveDirection.Y < 0f))
		{
			isDoubleJumping = false;
		}

		if (!walking) //Remove '!' to have the run button required for player to run
		{
			maxMoveSpeedWithoutRolling = RUNSPEED;
		}
		else
		{
			maxMoveSpeedWithoutRolling = WALKSPEED;
		}

		bool travellingDownardsThisFrame = Transform.Origin.Y < previousFrameYPosition;

		if(grounded && landedThisFrame)
		{
			landedThisFrame = false;
		}


		previousFrameYPosition = Transform.Origin.Y;

		timerToRollAgain -= deltafloat;

		HandleAnimation();

		Vector3 origin = Transform.Origin + Vector3.Up;
		float downdistance = 1.4f;
		//Vector3 downEndpoint = origin + (-Vector3.Up * downdistance);
		Vector3 shadowEndpoint = origin + (-Vector3.Up * (downdistance * 50f));
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		//Query for placing shadow position
		var shadowQuery = PhysicsRayQueryParameters3D.Create(origin, shadowEndpoint);
		Godot.Collections.Dictionary shadowRay = spaceState.IntersectRay(shadowQuery);

		var forwardWallQuery = PhysicsRayQueryParameters3D.Create(origin, origin + this.Basis.Z/2f);
		Godot.Collections.Dictionary forwardWallRay = spaceState.IntersectRay(forwardWallQuery);

		if(forwardWallRay.Count > 0)
		{
			Vector3 normal = (Vector3)forwardWallRay["normal"];

			moveDirection += new Vector3(normal.X, 0f, normal.Z) * 2.4f * 360.0f * (float)delta;
		}

		if(shadowRay.Count > 0)
		{
			shadow.Visible = true;

			Vector3 hitposition = (Vector3)shadowRay["position"];

			var translatedTransform = new Transform3D(Transform.Basis, new Vector3(hitposition.X, hitposition.Y, hitposition.Z));

			shadow.Transform = translatedTransform;
		} else
		{
			shadow.Visible = false;
		}

		if (IsOnFloor() && moveDirection.Y <= 0.0f)
		{
			disableJumpTimer.Stop();

			canDoDoubleJumpInput = true;

			if (!grounded && !landedThisFrame)
			{
				landedThisFrame = true;
			}

			grounded = true;
			stairBump = false;
		}
		else if(!stairBump)
		{ 
			if(grounded)
			{
				leftLedgeThisFrame = true;
			}

			grounded = false;
		}

		if (grounded && moveDirection.Y < -STICK_TO_GROUND_FORCE)
		{
			moveDirection.Y = -STICK_TO_GROUND_FORCE;
		}

		if(leftLedgeThisFrame && moveDirection.Y < 0.0f)
		{
			moveDirection.Y = 0.0f;
		}

		var lastCollision = GetLastSlideCollision();

		//if(lastCollision != null)
		//{
		//moveDirection -= lastCollision.GetNormal();
		//}

		//At this point we know we've already scaled by delta properly.
		//Beware that you'll need to scale by delta in _Process every time you add to the player's position as opposed to assigning moveDirection components directly to fixed values.
		Velocity = moveDirection;
		Vector3 testDirection = new Vector3(Velocity.X, 0.0f, Velocity.Z);

		bool willHitWall = TestMove(this.Transform, testDirection.Normalized());

		if(!StairStepUp(delta) && willHitWall)
		{
			Velocity = new Vector3(0.0f, Velocity.Y, 0.0f);
		}

		Velocity = moveDirection;

		if(!dead)
		{
			MoveAndSlide();
		}


		playerModel.Scale = adjustedPlayerModelScale;

		adjustedPlayerModelScale = adjustedPlayerModelScale.Lerp(initialPlayerModelScale, 11f * deltafloat);

		timeSinceLastJump += (float)delta;

		leftLedgeThisFrame = false;
	}

	public void SquashAndStretch(float squashFactor)
	{
		if(dead)
		{
			adjustedPlayerModelScale = initialPlayerModelScale;
			return;
		}
		adjustedPlayerModelScale = new Vector3(initialPlayerModelScale.X * squashFactor, initialPlayerModelScale.Y / squashFactor, initialPlayerModelScale.Z * squashFactor);
	}

	private void HandleAnimation()
	{
		haroldVisuals.Visible = GameManager.Instance.selectedCharacter == GameManager.SelectedCharacter.HAROLD;
		umaVisuals.Visible = GameManager.Instance.selectedCharacter == GameManager.SelectedCharacter.UMA;

		if(dead)
		{
			haroldAnimationPlayer.Play("death");
			automatonLungAnimationPlayer.Play("death");
			return;
		}

		if(isRolling)
		{
			haroldAnimationPlayer.Play("frontFlipGrounded", 0.2f, 3f);
			automatonLungAnimationPlayer.Play("frontFlipGrounded", 0.2f, 3f);
		}

		float trueMoveSpeed = (moveSpeed/6f) + (boostAddition.Abs().Length()/20f);

		if(trueMoveSpeed >= 4.5f && boostAddition != Vector3.Zero)
		{
			speedParticles.Emitting = true;
		} else {
			speedParticles.Emitting = false;
		}

		if(grounded && trueMoveSpeed > 0f && !isRolling)
		{
			if(!analogGreaterThan0 || walking)
			{
				haroldAnimationPlayer.Play("walk",0.05f, trueMoveSpeed);
				automatonLungAnimationPlayer.Play("walk",0.05f, trueMoveSpeed);
			} else 
			{
				haroldAnimationPlayer.Play("Run",0.2f,trueMoveSpeed/1.4f);
				automatonLungAnimationPlayer.Play("Run",0.2f,trueMoveSpeed/1.4f);
			}
		}

		if(grounded && moveSpeed <= 0.22f && !analogGreaterThan0 && !isRolling)
		{
			haroldAnimationPlayer.Play("Idle", -1, 1f);
			automatonLungAnimationPlayer.Play("Idle", -1, 1f);
		}

		if(!grounded && !isRolling)
		{
			if(moveDirection.Y > 0f && canDoDoubleJumpInput)
			{
				haroldAnimationPlayer.Play("jumpPose", 0.2f, 1f);
				automatonLungAnimationPlayer.Play("jumpPose", 0.2f, 1f);
			} else if (!canDoDoubleJumpInput && moveDirection.Y > 0f){
				haroldAnimationPlayer.Play("frontflip", 0.2f, 3f);
				automatonLungAnimationPlayer.Play("frontflip", 0.2f, 3f);
			} else if (moveDirection.Y <= 0f)
			{
				haroldAnimationPlayer.Play("fall2", 0.2f, 1f);
				automatonLungAnimationPlayer.Play("fall2", 0.2f, 1f);
			}
		}
	}

	private void HandleInput()
	{
		Vector2 velocity = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		horizontalaxis = velocity.X;
		verticalaxis = velocity.Y;
		jumpButton = Input.IsActionJustPressed("Jump");
		rollButton = Input.IsActionPressed("Roll");
		walking = Input.IsActionPressed("Walk");
	}

	private void Jump()
	{
		if(dead)
		{
			return;
		}
		//Cancel the roll when grounded, rolling, and jumping so that the player doesn't automatically use their double jump
		if(isRolling && grounded)
		{
			CancelRoll();
		}

		timeSinceLastGrounded += COYOTE_TIME*2f;

		gravityScale = GRAVITY_SCALE_UPWARDS_JUMP;
		moveDirection.Y = 0.0001f;
		grounded = false;

		GruntSound();
		moveDirection += jumpVelocity * Vector3.Up;

		slopePushVector.Y = 0f;
	}

	private void GruntSound(){
		if(!(bool)GameManager.Instance.optionsManager.options["soundEnabled"] || GameManager.Instance.selectedCharacter == GameManager.SelectedCharacter.UMA)
		{
			return;
		}
		playerSoundPlayer.Stop();
		playerSoundPlayer.VolumeDb = Mathf.Lerp(-60.0f, playerSoundPlayer.VolumeDb, (float)GameManager.Instance.optionsManager.options["soundScale"]);
		playerSoundPlayer.Stream = grunts[(int)GD.RandRange(0, grunts.Length-1)];
		playerSoundPlayer.PitchScale = (float)GD.RandRange(0.85f, 1.1f);
		playerSoundPlayer.Play();
	}

	private void DeathSound()
	{
		if(!(bool)GameManager.Instance.optionsManager.options["soundEnabled"] || GameManager.Instance.selectedCharacter == GameManager.SelectedCharacter.UMA)
		{
			return;
		}
		playerSoundPlayer.Stop();
		playerSoundPlayer.VolumeDb = Mathf.Lerp(-60.0f, playerSoundPlayer.VolumeDb, (float)GameManager.Instance.optionsManager.options["soundScale"]);
		playerSoundPlayer.Stream = deathSound;
		playerSoundPlayer.PitchScale = 1.0f;
		playerSoundPlayer.Play();
	}

	public void ChompSound(){
		if(!(bool)GameManager.Instance.optionsManager.options["soundEnabled"] || GameManager.Instance.selectedCharacter == GameManager.SelectedCharacter.UMA)
		{
			return;
		}
		playerSoundPlayer.Stop();
		playerSoundPlayer.VolumeDb = Mathf.Lerp(-60.0f, playerSoundPlayer.VolumeDb, (float)GameManager.Instance.optionsManager.options["soundScale"]);
		playerSoundPlayer.Stream = chomps[(int)GD.RandRange(0, chomps.Length-1)];
		playerSoundPlayer.PitchScale = (float)GD.RandRange(0.85f, 1.1f);
		playerSoundPlayer.Play();
	}

	private void CancelRoll()
	{
		rollButton = false;
		isRolling = false;
		canDoDoubleJumpInput = true;
		hasAttackedBeforeHittingGround = false;
	}

	public void Boost(float power, Vector3 direction)
	{
		timeSinceBoost = 0.0f;
		boostAddition = direction * power;
		//leftOverRollSpeed += 20f;
	}

	private void Roll(float deltafloat)
	{
		if(hasAttackedBeforeHittingGround || timerToRollAgain > 0f || (moveSpeed < 2f && grounded))
		{
			return;
		}

		GameManager.Instance.soundManager.PlaySound(rollSound);
		
		//**SUPER JUMP**
		//Some fancy effects to let the player know they are doing the jumproll technique correctly (in which the player gains more height by rolling immediately after a double jump).
		if(timeSinceLastJump < 0.35f && !grounded)
		{
			GameManager.Instance.soundManager.PlaySound(sparkleSound);
			superJumpParticles.Restart();
			superJumpParticles.Emitting = true;

			//We also provide a small upwards boost to give it some extra oomph
			this.moveDirection.Y += 7.8f;
		}


		timerToRollAgain = 0.8f;
		SquashAndStretch(1.8f);

		isDoubleJumping = true;
		isRolling = true;
		hasAttackedBeforeHittingGround = true;

		if (!grounded)
		{
			if(moveDirection.Y < 0f)
				moveDirection.Y = 18f;
			forwardLaunch = 45f;
		}
		else
		{
			forwardLaunch = 55f;
		}
	}

	public void TakeDamage(){
		GruntSound();
		DeathWithAnimation();
	}

	public void DeathInstant()
	{
		GameManager.Instance.saveManager.saveDataValues.deathCount += 1;

		GameManager.Instance.portalFlashEffect.DoFlash(new Color(0f, 0f, 0f, 1f));
		GameManager.Instance.playermovement.forwardLaunch = 0.0f;
		GameManager.Instance.playermovement.boostAddition = Vector3.Zero;

		GameManager.Instance.playermovement.cameraNode.cameraMode = PlayerCamera.CameraMode.NORMAL;

		GameManager.Instance.playermovement.cameraNode.GoBehindPlayer();
		dead = false;

		GameManager.Instance.levelManager.LoadScene(GameManager.Instance.levelManager.currentScene, true);
	}

	public void DeathWithAnimation()
	{
		if(dead)
		{
			return;
		}

		DeathSound();

		dead = true;
		GetTree().CreateTimer(2f, false).Timeout += DeathInstantDelay;
	}

	private void DeathInstantDelay()
	{
		if(dead)
		DeathInstant();
	}

	/// <summary>
	/// Adjusts 'moveDirection' as necessary to accomodate stair stepping.
	/// Returns 'true' if a stairstepus in necessary and 'false' otherwise.
	/// </summary> <summary>
	/// 
	/// </summary>
	private bool StairStepUp(double delta)
	{
		if(!grounded)
		{
			return false;
		}

		float maxStairHeight = 0.5f;
		bool retVal = false;

		//TODO : Better stair stepping
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		Vector3 flatMD = new Vector3(moveDirection.X, 0f, moveDirection.Z);
		Vector3 stairQueryOrigin = new Vector3(this.GlobalPosition.X, this.GlobalPosition.Y + maxStairHeight, this.GlobalPosition.Z) + flatMD.Normalized();
		var forwardQuery = PhysicsRayQueryParameters3D.Create(stairQueryOrigin, stairQueryOrigin + (Vector3.Down * (maxStairHeight - 0.001f)));
		Godot.Collections.Dictionary forwardRay = spaceState.IntersectRay(forwardQuery);

		if(forwardRay.Count > 0)
		{
			float fmdl = flatMD.Length();

			if(fmdl > 2.0f)
			{
				Vector3 hitPoint = (Vector3)forwardRay["position"];
				float stepHeight = maxStairHeight - hitPoint.DistanceTo(stairQueryOrigin);
				stairBump = true;

				this.GlobalPosition = new Vector3(this.GlobalPosition.X, hitPoint.Y, this.GlobalPosition.Z) + (Vector3.Up/16f);
				this.moveDirection.Y = (fmdl * fmdl) * 0.07f * (stepHeight / maxStairHeight);

				float stairBoostLimit = 20.0f;
				if(this.moveDirection.Y > stairBoostLimit)
				{
					this.moveDirection.Y = stairBoostLimit;
				}

				retVal = true;
			}

			grounded = true;

			return retVal;
		}

		return false;
	}
}
