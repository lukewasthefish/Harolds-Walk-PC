using Godot;
using System;

public partial class PauseMenu : Control
{
	public override void _Ready()
	{
		GameManager.Instance.pauseMenu = this;
	}

	public void Resume(){
		GameManager.Instance.TogglePause();
	}

	public void Save(){
		GameManager.Instance.saveManager.Save();
	}

	public void RestartLevel(){
		GameManager.Instance.Resume();

		if(Extensions.IsValid(GameManager.Instance.playermovement))
			GameManager.Instance.playermovement.DeathInstant();
	}

	public void LevelSelect(){
		GameManager.Instance.Resume();
		GameManager.Instance.levelManager.LoadScene("level_select");
	}

	public void MainMenu(){
		GameManager.Instance.Resume();
		GameManager.Instance.levelManager.LoadScene("main_menu");
	}
}
