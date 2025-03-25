using Godot;
using System;
using System.IO;

/// <summary>
/// Will remain in existence for the duration of the games runtime including menus from start to game close.
/// </summary>
public partial class LevelManager : Node3D
{
	public Node level = null;
	private PackedScene nextLevel = null;
	private Node player = null;
	private Node loadingScene = null;

	public string currentScene;
	public string previousScene = "";

	public bool inGameplayScene = false;
	private bool loadingSceneInProgress = false; //necessary so we don't layer scenes on top of one another.

	public override void _Ready()
	{
		GameManager.Instance.levelManager ??= this;

		if(GetChildren().Count > 0)
		{
			level = GetChildren()[0];
		}
	}

	public void LoadNewGameScene()
	{
		GameManager.Instance.selectedCharacter = GameManager.SelectedCharacter.HAROLD;
		GameManager.Instance.levelManager.LoadScene("WaterTower", true);
	}

	/// <summary>
	/// Always use this to load levels
	/// /// </summary>
	/// <param name="scenename"></param>
	/// <param name="gameplayScene"></param>
	public async void LoadScene(string scenename, bool gameplayScene = false, bool clearLevel = false, bool ignoresave = false)
	{
		if(loadingSceneInProgress)
		{
			return;
		}

		loadingSceneInProgress = true;

		this.previousScene = this.currentScene;


		this.inGameplayScene = gameplayScene;
		
		string scenesFilePath = "res://Scenes";
		string scenePath = $"{scenesFilePath}/{scenename}.tscn";
		string playerPath = $"{scenesFilePath}/PlayerEssentials.tscn";

		if(Extensions.IsValid(loadingScene))
		{
			RemoveChild(loadingScene);
			loadingScene.Free();
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		//GD.Print($"Loading scene at path : {scenePath}");
		PackedScene loadingScreen = (PackedScene)GD.Load($"{scenesFilePath}/LoadingScreen.tscn");
		loadingScene = loadingScreen.Instantiate();
		AddChild(loadingScene);

		//This is here to ensure that the loading screen is rendered after it is loaded.
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		if(ResourceLoader.Exists(scenePath))
		{
			nextLevel = (PackedScene)GD.Load(scenePath);
		} else 
		{
			GD.PrintErr($"scene {scenename} does not exist.");
			if(GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.ContainsKey(scenename))
			{
				GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.Remove(scenename);
			}
			scenePath = $"{scenesFilePath}/level_select.tscn";
			gameplayScene = false;
			clearLevel = false;
			nextLevel = (PackedScene)GD.Load(scenePath);
		}

		if(Extensions.IsValid(GameManager.Instance.playermovement))
			GameManager.Instance.playermovement.movementEnabled = true;

		GameManager.Instance.pickupsInScene.Clear();

		this.currentScene = scenename;

		GameManager.Instance.saveManager.UpdateAllPickupCounts();

		if(Extensions.IsValid(GameManager.Instance.playermovement) && Extensions.IsValid(GameManager.Instance.playermovement.cameraNode))
			GameManager.Instance.playermovement.cameraNode.cameraMode = PlayerCamera.CameraMode.NORMAL;

		if(clearLevel)
		{
			GameManager.Instance.saveManager.saveDataValues.levelsCleared[currentScene] = true;
		}

		if(Extensions.IsValid(player))
		{
			RemoveChild(player);
			player.Free();
		}	

		//Some guy on reddit said to add this; and I quote :

		//" [–]TheDuriel 3 points 1 year ago* 
   		//get_parent().remove_child(self)
    	//new_parent.add_child(self)
		//Despite what some contributor tried to tell me recently. You will need to await an idle frame between these two lines."

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		if(gameplayScene)
		{
			PackedScene playerPacked = (PackedScene)GD.Load(playerPath);
			player = playerPacked.Instantiate();
			AddChild(player);
		}

		if(Extensions.IsValid(level))
		{
			RemoveChild(level);
			level.Free();
		}

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		level = nextLevel.Instantiate();
		//Reset SaveID counter right before adding a level that could potentially introduce a new hierarchy of pickups.
		GameManager.Instance.saveManager.ResetSaveIDCounter();
		AddChild(level);

		if(Extensions.IsValid(GameManager.Instance.playerUIManager))
			GameManager.Instance.playerUIManager.levelIconScreenshotViewportVisibility.Visible = true;

		if(gameplayScene)
		{
			GameManager.Instance.playermovement.timeSinceBoost = 2000; //Disable boost curve to prevent it from carrying over from each level; this could have become a speedrunning exploit.
			if(!GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(scenename))
			{
				GameManager.Instance.saveManager.saveDataValues.crownCollectedData.Add(scenename, new System.Collections.Generic.Dictionary<uint, bool>());
			}

			if(!GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(scenename))
			{
				GameManager.Instance.saveManager.saveDataValues.donutCollectedData.Add(scenename, new System.Collections.Generic.Dictionary<uint, bool>());
			}

			if(!GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(scenename))
			{
				GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.Add(scenename, new System.Collections.Generic.Dictionary<uint, bool>());
			}

			GameManager.Instance.saveManager.saveDataValues.levelsUnlocked[scenename] = true;
		}

		if(!ignoresave)
			GameManager.Instance.saveManager.Save();


		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if(gameplayScene && GameManager.Instance.gameMode == GameManager.GameMode.NORMAL)
		{
			Screenshot();

			//TODO : Reimplement, figure out why the code as-is was failing in certain strange corner cases.
			//This part of the program caused more issues for me in testing than almost anything else for a while.

			////Uncollect all pickups, basically resetting the save data of this one particular level if something about it seems incorrect or corrupted
			//if(!CheckCollectibleValidityInCurrentLevel(scenename))
			//{
				//GD.Print($"Resetting save data for scene {scenename} because something changed in the level since player was last here.");

				//GameManager.Instance.saveManager.saveDataValues.crownCollectedData[scenename] = new System.Collections.Generic.Dictionary<uint, bool>();
				//GameManager.Instance.saveManager.saveDataValues.donutCollectedData[scenename] = new System.Collections.Generic.Dictionary<uint, bool>();
				//GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[scenename] = new System.Collections.Generic.Dictionary<uint, bool>();

				////Re-count pickups in the scene and update the info.
				//GameManager.Instance.saveManager.ResetSaveIDCounter();
				//foreach(Pickup p in GameManager.Instance.pickupsInScene)
				//{
					//p.Initialize();
				//}

				//GameManager.Instance.playerUIManager.UpdateGuiCounters();
			//}

			foreach(Pickup p in GameManager.Instance.pickupsInScene)
			{
				p.Initialize();

				if(Extensions.IsValid(GameManager.Instance.playerUIManager))
				GameManager.Instance.playerUIManager.UpdateGuiCounters();
			}
		}

		if(Extensions.IsValid(loadingScene))
			loadingScene.Free();

		//This wait should be here so that the time attack timer doesn't overlap with any loading times.
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			GameManager.Instance.ResetTimeAttackTimer();
		}

		loadingSceneInProgress = false;
	}

	/// <summary>
	/// In the currently loaded gameplay level check if the players collectibles align with what actually exists on that level.
	/// For example if the player has 15 crowns and there are only 3 crowns on the level this will return false.
	/// 
	/// If things do line up correctly we simply return true.
	/// </summary>
	private bool CheckCollectibleValidityInCurrentLevel(string scenename)
	{
		if(!GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(scenename))
		{
			GameManager.Instance.saveManager.saveDataValues.crownCollectedData[scenename] = new System.Collections.Generic.Dictionary<uint, bool>();
		}

		if(!GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(scenename))
		{
			GameManager.Instance.saveManager.saveDataValues.donutCollectedData[scenename] = new System.Collections.Generic.Dictionary<uint, bool>();
		}

		if(!GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(scenename))
		{
			GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[scenename] = new System.Collections.Generic.Dictionary<uint, bool>();
		}

		int numberOfCrownsInScene = GameManager.Instance.saveManager.saveDataValues.crownCollectedData[scenename].Keys.Count;
		int numberOfDonutsInScene = GameManager.Instance.saveManager.saveDataValues.donutCollectedData[scenename].Keys.Count;
		int numberOfPortalsInScene = GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[scenename].Keys.Count;

		int sum = numberOfCrownsInScene + numberOfDonutsInScene + numberOfPortalsInScene;

		if (GameManager.Instance.saveManager.GetMostRecentObjectSaveID() != sum)
		{
			return false;
		}

		return true;
	}

	private void Screenshot()
	{
		GameManager.Instance.saveManager.ScreenshotForCurrentLevel();
	}
}
