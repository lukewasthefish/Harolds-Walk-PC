using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class PlayerBlink : Node
{
	[Export]
	private Node3D eyelids;

	private Timer blinkWaitTimer;
	private Timer blinkDurationTimer;

	private RandomNumberGenerator rng = new RandomNumberGenerator();

	private bool enabled = true;

	public override void _Ready()
	{
		blinkWaitTimer = new Timer();
		blinkWaitTimer.OneShot = true;
		Callable selfCallable = new Callable(this, "CloseEyes");
		blinkWaitTimer.Connect("timeout", selfCallable);
		AddChild(blinkWaitTimer);

		blinkDurationTimer = new Timer();
		blinkDurationTimer.OneShot = true;
		Callable selfCallable2 = new Callable(this, "OpenEyes");
		blinkDurationTimer.Connect("timeout", selfCallable2);
		AddChild(blinkDurationTimer);

		OpenEyes();
	}

	private void CloseEyes()
	{
		if(!enabled)
		{
			return;
		}

		eyelids.Visible = true;

		blinkDurationTimer.Start(rng.RandfRange(0.1f,0.25f));
	}

	private void OpenEyes()
	{
		if(!enabled)
		{
			return;
		}

		eyelids.Visible = false;

		blinkWaitTimer.Start(rng.RandfRange(0.4f, 3f));
	}

	public void CloseEyesAndDisable()
	{
		eyelids.Visible = true;
		enabled = false;
	}
}
