using Godot;
using System;

public partial class CrownCage : Node
{
	[Export]
	private Label3D label3D;

	[Export]
	private int requiredCrownsToRemove = 30;
	[Export]
	private int donutsRequiredToRemove = 280;

	public enum RemoveType {CROWNS, DONUTS}
	[Export]
	public RemoveType removeType = RemoveType.CROWNS;

	public override void _Ready()
	{
		base._Ready();

		switch(removeType)
		{
			case RemoveType.CROWNS:
			label3D = (Label3D)FindChild("Label3D");

			if(Extensions.IsValid(label3D))
			label3D.Text = $"BLOCKED BY FIREWALL: haroldswalk.x86_64 requires {requiredCrownsToRemove} .crown files at a minimum to bypass.";
			break;

			case RemoveType.DONUTS:
			label3D = (Label3D)FindChild("Label3D");
			if(Extensions.IsValid(label3D))
			{
				label3D.Text = $"BLOCKED BY FIREWALL: Not enough memory allocated. {donutsRequiredToRemove} bytes must be allocated at minimum to bypass.";
			}
			break;
		}
	}

	public override void _Process(double delta)
	{
		if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			this.QueueFree();
			return;
		}

		switch(removeType)
		{
			case RemoveType.CROWNS:
			if(GameManager.Instance.crownCount >= requiredCrownsToRemove)
			{
				this.QueueFree();
			}
			break;

			case RemoveType.DONUTS:
			if(GameManager.Instance.donutCount >= donutsRequiredToRemove)
			{
				this.QueueFree();
			}
			break;
		}
	}
}
