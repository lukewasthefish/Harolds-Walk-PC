using Godot;
using System;

public partial class levelLoadButton : Button
{
	public string levelToLoad;
	public string levelInfoText; //Used by the label in the level menu scene to display information about the corresponding level tied to this button

	public Texture2D levelIcon;

	public levelLoadButton(string levelToLoad){
		this.levelToLoad = levelToLoad;

		this.Pressed += LoadLevelFromButton;
	}

	private void LoadLevelFromButton(){
		GameManager.Instance.levelManager.LoadScene(levelToLoad, true);
	}
}
