using Godot;
using System;

public partial class cutscene : Node
{
	[Export]
	private float length = 1f;

	[Export]
	private string nextScene;

	[Export]
	private bool gameplayScene = true;

	private SceneTreeTimer sceneTreeTimer;

	public override void _Ready()
	{
		sceneTreeTimer = GetTree().CreateTimer(length);
		sceneTreeTimer.Timeout += LoadNextScene;

		GameManager.Instance.saveManager.Save();
	}

	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("Pause"))
		{
			LoadNextScene();
		}
	}

	private void LoadNextScene()
	{
		sceneTreeTimer.Timeout -= LoadNextScene; //Making sure this doesn't get triggered again after the cutscene is over
		GameManager.Instance.levelManager.LoadScene(nextScene, gameplayScene);
	}
}
