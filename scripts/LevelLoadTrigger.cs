using Godot;
using System;

public partial class LevelLoadTrigger : Area3D
{
	[Export]
	private string levelName = "WaterTower";
	
	[Export]
	private Color flashColor = new Color(0f, 1f, 0f, 1f);

	[Export]
	private AudioStream sound; //Leave empty if you don't want to inlcude a sound

	[Export]
	private bool gameplayScene = true;

	[Export]
	private bool unlockAchievement = false;

	[Export]
	private string achievementName = "tunnel"; //DON'T change this

	[Export]
	private bool includeInTimeAttack = true;

	//An optional secret exit trigger (in the case of Harold's Walk, used for the portals.)
	[Export]
	private SecretExitPickup secretExitPickup;

	private bool triggered = false;

	public override void _Ready()
	{
		base._Ready();

		this.AreaEntered += _on_area_entered;

		if(!includeInTimeAttack && GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			QueueFree();
			return;
		}
	}

	/// <summary>
	/// Changes the root scene to the scene at the path defined in the editor
	/// </summary>
	private void LoadLevel()
	{
		if(Extensions.IsValid(secretExitPickup))
		{
			secretExitPickup.CollectAction();
		}

		if(unlockAchievement)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement(achievementName);
		}
		
		if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			double result = GameManager.Instance.GetTimeAttackTime();

			if(!GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes.ContainsKey(GameManager.Instance.levelManager.currentScene))
			{
				GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[GameManager.Instance.levelManager.currentScene] = Extensions.GetDefaultTimeAttackTime();
			}

			if(result < GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[GameManager.Instance.levelManager.currentScene])
			{
				GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[GameManager.Instance.levelManager.currentScene] = GameManager.Instance.GetTimeAttackTime();
			}

			GameManager.Instance.levelManager.LoadScene("level_select", false, false);
		} else 
		{
			GameManager.Instance.levelManager.LoadScene(levelName, gameplayScene, true);
		}

		GameManager.Instance.portalFlashEffect.DoFlash(flashColor);

		if(this.sound != null)
		{
			GameManager.Instance.soundManager.PlaySound(sound);
		}

	}
	
	/// <summary>
	/// Check for collisions against player
	/// </summary>
	/// <param name="area"></param>
	private void _on_area_entered(Area3D area)
	{
		if(triggered)
		{
			return;
		}

		if(area.IsInGroup("Player"))
		{
			triggered = true;
			LoadLevel();
		}
	}
}
