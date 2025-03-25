using Godot;
using GodotSteam;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	//Classes
	public static GameManager Instance;
	public playermovement playermovement;
	public LevelManager levelManager;
	public PortalFlashEffect portalFlashEffect;
	public SaveManager saveManager;
	public MusicManager musicManager;
	public SoundManager soundManager;
	public SteamManager steamManager;
	public PlayerUIManager playerUIManager;
	public OptionsManager optionsManager;

	public int deathPoint = 0; //Y axis position under which the player will be immediately killed to prevent noclipping issues (and to essentially generate death triggers where forgotten)

	public PauseMenu pauseMenu;

	public int crownCount = 0;
	public int donutCount = 0;
	public int portalCount = 0;

	public List<Pickup> pickupsInScene = new List<Pickup>();

	public enum GameMode {NORMAL, TIME_ATTACK}
	public GameMode gameMode = GameMode.NORMAL;

	private double levelTime = 0.0d;

	public enum SelectedCharacter{HAROLD, UMA};
	public SelectedCharacter selectedCharacter = SelectedCharacter.HAROLD;

	public override void _EnterTree(){
		this.ProcessMode = ProcessModeEnum.Always;
		Instance ??= this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if(this != Instance)
		{
			return;
		}

		if(Input.IsActionJustPressed("Pause") && Extensions.IsValid(GameManager.Instance.playermovement))
		{
			TogglePause();
		}

		if(Extensions.IsValid(pauseMenu))
		pauseMenu.Visible = GetTree().Paused;

		if(!GetTree().Paused && Extensions.IsValid(GameManager.Instance.playermovement))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		} else 
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		if(GameManager.Instance.gameMode == GameMode.TIME_ATTACK && !GetTree().Paused)
		{
			levelTime += delta;
		}
	}

	public void TogglePause(){
		GetTree().Paused = !GetTree().Paused;
	}

	public void Resume(){
		GetTree().Paused = false;
	}

	public void Pause(){
		GetTree().Paused = true;
	}

	public void ResetTimeAttackTimer()
	{
		levelTime = 0.0d;
	}

	public double GetTimeAttackTime()
	{
		return levelTime;
	}
}
