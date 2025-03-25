using Godot;
using System;

/// <summary>
/// A collection of functions that can be called from Control nodes.
/// For example the title menu can call one of these functions to start the game.
/// </summary>
public partial class UIBindings : Node
{
	[Export]
	private Label newGameErrorText;
	public void LevelSelect(){
		GameManager.Instance.levelManager.LoadScene("level_select", false, false, true);
	}

	public void NewGame()
	{
		GameManager.Instance.gameMode = GameManager.GameMode.NORMAL;

		for(int i = 0; i < GameManager.Instance.saveManager.maxFiles; i++)
		{
			if(!FileAccess.FileExists(GameManager.Instance.saveManager.GetFileSavePath() + i + ".dat"))
			{
				GameManager.Instance.saveManager.saveFileIndex = i;
				GameManager.Instance.saveManager.saveDataValues = new SaveManager.SaveDataValues();
				GameManager.Instance.levelManager.LoadNewGameScene();
				return;
			}
		}

		//If we get this far we know that we are at the save file limit and should give a warning to the player,
		//Letting them know they need to delete a save file to make room.
		newGameErrorText.Visible = true;
	}


	public void ReturnToMainMenu(){
		GameManager.Instance.levelManager.LoadScene("main_menu", false, false, true);
	}

	private void FileSelect(){
		GameManager.Instance.levelManager.LoadScene("file_select", false, false, true);
	}

	private void AboutMenu()
	{
		GameManager.Instance.levelManager.LoadScene("about_menu", false, false, true);
	}

	private void HelpMenu()
	{
		GameManager.Instance.levelManager.LoadScene("help_menu", false, false, true);
	}

	private void Credits()
	{
		GameManager.Instance.levelManager.LoadScene("credits", false, false, true);
	}

	public void LoadGame(){
		if(FileAccess.FileExists(GameManager.Instance.saveManager.GetCurrentFileSavePath()) && GameManager.Instance.saveManager.Load(GameManager.Instance.saveManager.saveFileIndex))
		{
			GameManager.Instance.saveManager.Load(GameManager.Instance.saveManager.saveFileIndex);
			GameManager.Instance.levelManager.LoadScene("level_select", false, false, true);
		} else 
		{
			GameManager.Instance.levelManager.LoadScene("WaterTower", false, false, true);
		}
	}

	public void SelectTimeAttackMode()
	{
		GameManager.Instance.gameMode = GameManager.GameMode.TIME_ATTACK;
		GameManager.Instance.levelManager.LoadScene("level_select", false, false, true);
	}

	public void SelectNormalMode()
	{
		GameManager.Instance.gameMode = GameManager.GameMode.NORMAL;
		GameManager.Instance.levelManager.LoadScene("level_select", false, false, true);
	}

	//Deletes save file with the corresponding index that saveManager has.
	public void DeleteSaveFile()
	{
		GameManager.Instance.saveManager.Delete(GameManager.Instance.saveManager.saveFileIndex);
		GameManager.Instance.levelManager.LoadScene("file_select", false, false, true);
	}

	/// <summary>
	/// Open the options menu
	/// </summary>
	public void Options()
	{
		GameManager.Instance.levelManager.LoadScene("options_menu", false, false, true);
	}

	/// <summary>
	/// Exits the game.
	/// </summary>
	public void Exit()
	{
		GetTree().Quit();
	}
}
