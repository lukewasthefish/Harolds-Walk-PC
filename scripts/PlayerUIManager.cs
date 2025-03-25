using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public partial class PlayerUIManager : Node
{
	[Export]
	private Label crownCountLabel;

	[Export]
	private Label donutCountLabel;

	[Export]
	private Control hideParent;

	[Export]
	private RichTextLabel timeAttackTimer;

	[Export]
	public SubViewportContainer levelIconScreenshotViewportVisibility;
	[Export]
	public Viewport levelIconScreenshotViewport;
	[Export]
	public LevelSelectIconCamera levelSelectIconCamera;

	public bool UIhiddenState = false; //If true all ui will be invisible
	private bool internalUIHiddenState = false;

    public override void _Ready()
    {
        base._Ready();

		GameManager.Instance.playerUIManager = this;

		GameManager.Instance.playerUIManager.UpdateGuiCounters();
    }

	public void UpdateGuiCounters()
	{
		if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			HideUI();
			return; //We don't need to update anything else if we're on time attack mode.
		}

		foreach(Pickup p in GameManager.Instance.pickupsInScene)
		{
			if(Extensions.IsValid(p))
			p.Initialize();
		}

		uint crownCollectedCount = 0;
		uint donutCollectedCount = 0;


		if(!GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene))
		{
			GameManager.Instance.saveManager.saveDataValues.crownCollectedData.Add(GameManager.Instance.levelManager.currentScene, new Dictionary<uint, bool>());
		}

		if(!GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene))
		{
			GameManager.Instance.saveManager.saveDataValues.donutCollectedData.Add(GameManager.Instance.levelManager.currentScene, new Dictionary<uint, bool>());
		}

		foreach(uint id in GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene].Keys){
			if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene][id])
			{
				crownCollectedCount += 1;
			}
		}
		if((uint)GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene].Keys.Count > 0){
			uint crownCollectedRatio = crownCollectedCount / (uint)GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene].Keys.Count;

			if(crownCollectedRatio >= 1)
			{
				crownCountLabel.AddThemeColorOverride("font_color", new Color(0,1,0,1));
			} else {
				crownCountLabel.AddThemeColorOverride("font_color", new Color(1,1,1,1));
			}
		}

		crownCountLabel.Text = $"{crownCollectedCount} / {GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene].Keys.Count}";

		foreach(uint id in GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene].Keys){
			if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene][id])
			{
				donutCollectedCount += 1;
			}
		}

		if((uint)GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene].Keys.Count > 0){
			uint donutCollectedRatio = donutCollectedCount / (uint)GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene].Keys.Count;

			if(donutCollectedRatio >= 1)
			{
				donutCountLabel.AddThemeColorOverride("font_color", new Color(0,1,0,1));
			} else {
				donutCountLabel.AddThemeColorOverride("font_color", new Color(1,1,1,1));
			}
		}

		donutCountLabel.Text = $"{donutCollectedCount} / {GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene].Keys.Count}";
	}

    public override void _Process(double delta)
	{
		hideParent.Visible = !internalUIHiddenState;
		if(Input.IsActionJustPressed("HideUI"))
		{
			ToggleUIHidden();
		}

		internalUIHiddenState = UIhiddenState;

		if(GetTree().Paused)
		{
			internalUIHiddenState = false;
		}

		timeAttackTimer.Visible = GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK;

		if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			timeAttackTimer.Text = Extensions.ParseTimeFromDouble(GameManager.Instance.GetTimeAttackTime());
		}
	}

	public void ToggleUIHidden()
	{
		UIhiddenState = !UIhiddenState;
	}

	public void HideUI()
	{
		UIhiddenState = true;
	}

	public void ShowUI()
	{
		UIhiddenState = false;
	}
}
