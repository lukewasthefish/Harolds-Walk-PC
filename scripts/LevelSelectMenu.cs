using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelSelectMenu : Node
{
	[Export]
	private Control rootButtonsContainer;

	[Export]
	private TextureRect levelIconDisplay;
	[Export]
	private Label levelLabel;

	[Export]
	private Label totalsLabel;

	private TimeAttackMedalRecords timeAttackMedalRecords = new TimeAttackMedalRecords();

	public override void _Ready()
	{
		int totalCrownsCollected = 0;
		int totalBytesCollected = 0; //Donuts is an internal keyword for 'bytes' which are the collectible pink rings with sprinkles the player collects
		int totalPortalsDiscovered = 0;

		//These values increase as the player discovers more levels that contain these things
		int totalCrownsInGame = 0;
		int totalBytesInGame = 0;
		int totalPortalsInGame = 0;

		int gold_medals = 0;

		double totalTime = 0.0d;

		List<string> levelsUnlocked = new List<string>();
		levelsUnlocked.AddRange(GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.Keys.ToArray());

		levelsUnlocked.Sort();

		foreach(string levelName in levelsUnlocked)
		{
			if(!GameManager.Instance.saveManager.saveDataValues.levelsUnlocked[levelName]){
				continue;
			}

			levelLoadButton button = new levelLoadButton(levelName);
			
			Image imageFromFile = Image.LoadFromFile(GameManager.Instance.saveManager.GetFileSaveDirectory() + levelName + ".png");

			if(imageFromFile != null)
			{
				Texture2D icon = ImageTexture.CreateFromImage(Image.LoadFromFile(GameManager.Instance.saveManager.GetFileSaveDirectory() + levelName + ".png"));

				if(icon != null)
					button.levelIcon = icon;
			}

			button.Text = Extensions.SceneNameToHumanFormat(levelName);

			//if(GameManager.Instance.saveManager.saveDataValues.levelsCleared.ContainsKey(key) && GameManager.Instance.saveManager.saveDataValues.levelsCleared[key])
			//{
				//button.levelInfoText += " [CLEARED]";
			//}

			//Show collectibles for each level
			button.levelInfoText += "\nCrowns collected : ";
			int crownsCollected = 0;
			if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(levelName))
			{
				foreach(uint id in GameManager.Instance.saveManager.saveDataValues.crownCollectedData[levelName].Keys)
				{
					if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData[levelName][id])
					{
						crownsCollected += 1;
					}
				}
			}
			int totalCrownsInLevel = GameManager.Instance.saveManager.saveDataValues.crownCollectedData[levelName].Count;
			button.levelInfoText += $"{crownsCollected} / {totalCrownsInLevel}";
			totalCrownsCollected += crownsCollected;
			totalCrownsInGame += totalCrownsInLevel;

			button.Alignment = HorizontalAlignment.Left;

			rootButtonsContainer.AddChild(button);

			button.levelInfoText += "\nBytes collected : ";
			int bytesCollected = 0;
			if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(levelName))
			{
				foreach(uint id in GameManager.Instance.saveManager.saveDataValues.donutCollectedData[levelName].Keys)
				{
					if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData[levelName][id])
					{
						bytesCollected += 1;
					}
				}
			}

			int totalBytesInLevel = GameManager.Instance.saveManager.saveDataValues.donutCollectedData[levelName].Count;
			button.levelInfoText += $"{bytesCollected} / {totalBytesInLevel}";
			totalBytesCollected += bytesCollected;
			totalBytesInGame += totalBytesInLevel;

			button.levelInfoText += "\nPortals discovered: ";
			int portalsCollected = 0;
			if(GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(levelName))
			{
				foreach(uint id in GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[levelName].Keys)
				{
					if(GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[levelName][id])
					{
						portalsCollected += 1;
					}
				}
			}

			if(!GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(levelName))
			{
				GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.Add(levelName, new System.Collections.Generic.Dictionary<uint, bool>());
			}

			int totalPortalsInLevel = GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[levelName].Count;
			button.levelInfoText += $"{portalsCollected} / {totalPortalsInLevel}";
			totalPortalsDiscovered += portalsCollected;
			totalPortalsInGame += totalPortalsInLevel;

			if(!GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes.ContainsKey(levelName))
			{
				//TODO : We should set records for each level manually for the player to beat.
				GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[button.levelToLoad] = Extensions.GetDefaultTimeAttackTime();
			}

			totalTime += GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[button.levelToLoad];

			if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK && GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes.ContainsKey(levelName))
			{
				TimeAttackMedalRecords.TimeRecord timeRecord = new TimeAttackMedalRecords.TimeRecord();
				if(timeAttackMedalRecords.records.ContainsKey(levelName))
					timeRecord = timeAttackMedalRecords.records[levelName];

				if(GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[levelName] < timeRecord.gold)
				{
					gold_medals += 1;
				}
			}

			if(gold_medals >= 31)
			{
				GameManager.Instance.steamManager.UnlockSteamAchievement("gold_medalist");
			}

			if(button.levelToLoad == GameManager.Instance.levelManager.previousScene)
			{
				button.GrabFocus();
			}
		}

		if(GameManager.Instance.gameMode == GameManager.GameMode.NORMAL)
		{
			totalsLabel.Text = $"Total crowns: {totalCrownsCollected}/{totalCrownsInGame}\nTotal bytes: {totalBytesCollected}/{totalBytesInGame}\nTotal portals: {totalPortalsDiscovered}/{totalPortalsInGame}";
		} else if (GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			totalsLabel.Text = $"Total: {Extensions.ParseTimeFromDouble(totalTime)}";
		}
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

		Control owner = GetViewport().GuiGetFocusOwner();

		if(owner is levelLoadButton levelLoadButton)
		{
			if(Extensions.IsValid(levelLoadButton) && levelLoadButton.levelIcon != null)
				levelIconDisplay.Texture = levelLoadButton.levelIcon;

			if(GameManager.Instance.gameMode == GameManager.GameMode.NORMAL)
			{
				levelLabel.RemoveThemeColorOverride("font_color");
				levelLabel.Text = levelLoadButton.levelInfoText;
			} else if (GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
			{
				double time = Extensions.GetDefaultTimeAttackTime();
				//Load best time for the current level if applicable
				if(GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes.ContainsKey(levelLoadButton.levelToLoad))
				{
					time = GameManager.Instance.saveManager.saveDataValues.timeAttackLevelTimes[levelLoadButton.levelToLoad];
					levelLabel.Text = Extensions.ParseTimeFromDouble(time);
				} else 
				{
					//just under 10 minutes will be the default display
					levelLabel.Text = Extensions.ParseTimeFromDouble(Extensions.GetDefaultTimeAttackTime());
				}

				//Check the rankings of the players time against the hard-coded records assigned in TimeAttackMedalRecords.cs
				if(timeAttackMedalRecords.records.ContainsKey(levelLoadButton.levelToLoad))
				{
					TimeAttackMedalRecords.TimeRecord timeRecord = timeAttackMedalRecords.records[levelLoadButton.levelToLoad];

					string medalText = " [RANK = UNELIGIBLE]";

					//TOOD : Colors for silver and bronze are not working.
					Color newColor = new Color(155.0f, 155.0f, 155.0f);
					if(time < timeRecord.gold)
					{
						medalText = " [RANK = GOLD]";

						newColor = new Color(247.0f, 197.0f, 0.0f);
					} else if(time < timeRecord.silver)
					{
						medalText = " [RANK = SILVER]";

						newColor = new Color(193.0f, 193.0f, 193.0f);
					} else if(time < timeRecord.bronze)
					{
						medalText = " [RANK = BRONZE]";

						newColor = new Color(142.0f, 93.0f, 68.0f);
					}  

					levelLabel.AddThemeColorOverride("font_color", newColor);

					levelLabel.Text += medalText;

					levelLabel.Text += $"\nBronze : {timeRecord.bronze} seconds\nSilver: {timeRecord.silver} seconds\nGold: {timeRecord.gold} seconds";
				}
			}
		} else{
			levelIconDisplay.Texture = null;
			levelLabel.Text = "";
		}

    }

    private void LoadLevelFromButton(string levelName)
	{
		GameManager.Instance.levelManager.LoadScene(levelName, true);
	}
}