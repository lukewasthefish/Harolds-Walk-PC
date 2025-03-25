using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

/// <summary>
/// Handles writing variables from GameManager to a file.
/// 
/// Our save format is in binary and is then decoded through the FileAccess class.
/// The binary comes in with a string label initiall that denotes what the upcoming variable is for.
/// Next the corresponding variable is loaded.
/// 
/// Each label string is a Pascal string which means that it shows us the length first.
/// </summary>
public partial class SaveManager : Node
{
	public int saveFileIndex = -1;

	public int maxFiles = 16;

	private string recentScene;
	public SaveDataValues saveDataValues = new SaveDataValues();

	[Export]
	private Control saveIndicator; //shows a save happened correctly

	//Increment as we assigne save ids to objects
	private uint currentObjectSaveID = 0;

	public override void _Ready()
	{
		GameManager.Instance.saveManager ??= this;
	}

	/// <summary>
	/// Get ready for a new scene full of objects that need unique save ids
	/// </summary>
	public void ResetSaveIDCounter()
	{
		currentObjectSaveID = 0;
	}

	public uint GetNewSaveID()
	{
		currentObjectSaveID += 1;
		return currentObjectSaveID;
	}

	public uint GetMostRecentObjectSaveID()
	{
		return currentObjectSaveID;
	}

	public bool Load(int saveFileIndex)
	{
		this.saveFileIndex = saveFileIndex;
		saveDataValues = new SaveDataValues();
		using Godot.FileAccess file = Godot.FileAccess.Open(GetCurrentFileSavePath() + ".dat", Godot.FileAccess.ModeFlags.Read);

		if(file == null)
		{
			GD.PrintErr(Godot.FileAccess.GetOpenError());
			return false;
		}

		while(file.GetPosition() < file.GetLength()){

			string label = file.GetPascalString();

			//GD.Print("loaded in label as " + label);

			switch(label)
			{
				case "crownCount":
				uint crownCount = file.Get32(); //DISABLED - leave here though for older saves to get parsed correctly
				break;

				case "deathCount":
				uint deathCount = file.Get32();
				saveDataValues.deathCount = deathCount;
				break;

				case "donutCount":
				uint donutCount = file.Get32(); //DISABLED - leave here though for older saves to get parsed correctly
				break;

				case "levelsUnlocked":
				LoadDictionaryStringBool(saveDataValues.levelsUnlocked, file);

				if(saveDataValues.levelsUnlocked.ContainsKey("final_cutscene"))
				{
					saveDataValues.levelsUnlocked.Remove("final_cutscene");
				}
				break;

				case "levelsCleared":
				LoadDictionaryStringBool(saveDataValues.levelsCleared, file);

				if(saveDataValues.levelsCleared.ContainsKey("final_cutscene"))
				{
					saveDataValues.levelsCleared.Remove("final_cutscene");
				}
				break;

				case "endingsWon":
				LoadDictionaryStringBool(saveDataValues.endingsWon, file);
				break;

				//NOTE : Collectible save loads must happen after levelsUnlocked has been loaded
				case "crownPositions":
				ushort numberOfLevels = file.Get16();
				
				//This loop must exist so that we can show crown labels for levels that do not contain crowns.
				foreach(string key in saveDataValues.levelsUnlocked.Keys)
				{
					if(!saveDataValues.crownCollectedData.ContainsKey(key))
					{
						saveDataValues.crownCollectedData.Add(key, new Dictionary<uint, bool>());
					}
				}

				for(int i = 0; i < numberOfLevels; i++)
				{
					string levelname = file.GetPascalString();

					if(!saveDataValues.crownCollectedData.ContainsKey(levelname))
					{
						saveDataValues.crownCollectedData.Add(levelname, new Dictionary<uint, bool>());
					}
					LoadDictionaryUintBool(saveDataValues.crownCollectedData[levelname], file);
				}
				break;

				case "donutPositions":
				numberOfLevels = file.Get16();
				
				//This loop must exist so that we can show crown labels for levels that do not contain crowns.
				foreach(string key in saveDataValues.levelsUnlocked.Keys)
				{
					if(!saveDataValues.donutCollectedData.ContainsKey(key))
					{
						saveDataValues.donutCollectedData.Add(key, new Dictionary<uint, bool>());
					}
				}

				for(int i = 0; i < numberOfLevels; i++)
				{
					string levelname = file.GetPascalString();

					if(!saveDataValues.donutCollectedData.ContainsKey(levelname))
					{
						saveDataValues.donutCollectedData.Add(levelname, new Dictionary<uint, bool>());
					}
					LoadDictionaryUintBool(saveDataValues.donutCollectedData[levelname], file);
				}
				break;

				case "portalPositions":
				numberOfLevels = file.Get16();
				
				foreach(string key in saveDataValues.levelsUnlocked.Keys)
				{
					if(!saveDataValues.portalDiscoveredData.ContainsKey(key))
					{
						saveDataValues.portalDiscoveredData.Add(key, new Dictionary<uint, bool>());
					}
				}

				for(int i = 0; i < numberOfLevels; i++)
				{
					string levelname = file.GetPascalString();

					if(!saveDataValues.portalDiscoveredData.ContainsKey(levelname))
					{
						saveDataValues.portalDiscoveredData.Add(levelname, new Dictionary<uint, bool>());
					}
					LoadDictionaryUintBool(saveDataValues.portalDiscoveredData[levelname], file);
				}
				break;

				case "timeAttackLevelTimes":
				LoadDictionaryStringDouble(saveDataValues.timeAttackLevelTimes, file);
				break;

				default:
				GD.PrintErr($"Unrecognized label '{label}'");
				return false;
			}
		}

		//Save data successfully loaded.
		return true;
	}

	private void LoadDictionaryUintBool(Dictionary<uint, bool> destination, Godot.FileAccess file)
	{
		ushort pairCount = file.Get16();

		for(int i = 0; i < pairCount; i++)
		{
			uint ui = file.Get32();

			bool loadedBool = file.Get16() == 1 ? true : false;
			destination[ui] = loadedBool;
		}
	}

	private void LoadDictionaryUlongBool(Dictionary<ulong, bool> destination, Godot.FileAccess file)
	{
		ushort pairCount = file.Get16();

		for(int i = 0; i < pairCount; i++)
		{
			ulong ul = file.Get64();

			bool loadedBool = file.Get16() == 1 ? true : false;
			destination[ul] = loadedBool;
		}
	}

	private void LoadDictionaryVector3Bool(Dictionary<Vector3, bool> destination, Godot.FileAccess file){
		ushort pairCount = file.Get16();

		for(int i = 0; i < pairCount; i++)
		{
			Vector3 v3 = new Vector3(file.GetFloat(), file.GetFloat(), file.GetFloat());

			bool loadedBool = file.Get16() == 1 ? true : false;
			destination[v3] = loadedBool;
		}
	}

	private void LoadDictionaryStringBool(Dictionary<string, bool> destination, Godot.FileAccess file){
		ushort pairCount = file.Get16();

		for(int i = 0; i < pairCount; i++)
		{
			string key = file.GetPascalString();
			ushort value = file.Get16();

			bool valueb = (value != 0);

			destination[key] = valueb;
		}
	}

	//TODO : Generalize some of these save functions using generics so we don't have to maintain as much code.
	private void LoadDictionaryStringDouble(Dictionary<string, double> destination, Godot.FileAccess file)
	{
		ushort pairCount = file.Get16();

		for(int i = 0; i < pairCount; i++)
		{
			string key = file.GetPascalString();
			double value = file.GetDouble();

			destination[key] = value;
		}
	}

	/// <summary>
	/// Writes all necessary save files to the corresponding save data the player has chosen or is using.
	/// </summary>
	public void Save(){
		string filePath = GetCurrentFileSavePath() + ".dat";

		if(filePath == ".dat")
		{
			return;
		}

		if(!Godot.FileAccess.FileExists(filePath))
		{
			//Initialize save data values
			saveDataValues = new SaveDataValues();
		}

		using Godot.FileAccess file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Write);

		recentScene = GameManager.Instance.levelManager.currentScene;

		//Write labels and their variables directly after writing the label
		file.StorePascalString("deathCount");
		file.Store32(saveDataValues.deathCount);

		file.StorePascalString("levelsUnlocked");
		saveDataValues.levelsUnlocked.TryAdd("WaterTower", true);
		SaveDictionaryStringBool(saveDataValues.levelsUnlocked, file);

		file.StorePascalString("levelsCleared");
		SaveDictionaryStringBool(saveDataValues.levelsCleared, file);

		file.StorePascalString("endingsWon");
		SaveDictionaryStringBool(saveDataValues.endingsWon, file);

		file.StorePascalString("crownPositions");
		file.Store16((ushort)saveDataValues.crownCollectedData.Count);

		foreach(string levelName in saveDataValues.crownCollectedData.Keys)
		{
			file.StorePascalString(levelName);
			SaveDictionaryUintBool(saveDataValues.crownCollectedData[levelName], file);
		}

		file.StorePascalString("timeAttackLevelTimes");
		SaveDictionaryStringDouble(saveDataValues.timeAttackLevelTimes, file);

		file.StorePascalString("donutPositions");
		file.Store16((ushort)saveDataValues.donutCollectedData.Count);
		foreach(string levelName in saveDataValues.donutCollectedData.Keys)
		{
			file.StorePascalString(levelName);
			SaveDictionaryUintBool(saveDataValues.donutCollectedData[levelName], file);
		}

		file.StorePascalString("portalPositions");
		file.Store16((ushort)saveDataValues.portalDiscoveredData.Count);
		foreach(string levelName in saveDataValues.portalDiscoveredData.Keys)
		{
			file.StorePascalString(levelName);
			SaveDictionaryUintBool(saveDataValues.portalDiscoveredData[levelName], file);
		}

		GetTree().CreateTimer(0.3f).Timeout += ShowSaveIndicator;
		GetTree().CreateTimer(1.4f).Timeout += HideSaveIndicator;

		//GD.Print("SaveAck");
	}

	public void ScreenshotForCurrentLevel()
	{
		if(!Extensions.IsValid(GameManager.Instance.playerUIManager) || !Extensions.IsValid(GameManager.Instance.playerUIManager.levelIconScreenshotViewport))
		{
			return;
		}

		Image img = GameManager.Instance.playerUIManager.levelIconScreenshotViewport.GetTexture().GetImage();

		img.Resize(256, 256);
		img.SavePng(GetFileSaveDirectory() + GameManager.Instance.levelManager.currentScene + ".png");

		img.Resize(128,128);
		img.SavePng(GetCurrentFileSavePath() + ".png");

		GameManager.Instance.playerUIManager.levelIconScreenshotViewportVisibility.Visible = false;
	}

	private void ShowSaveIndicator()
	{
		if(GameManager.Instance.levelManager.inGameplayScene)
			saveIndicator.Visible = true;
	}

	private void HideSaveIndicator()
	{
		saveIndicator.Visible = false;
	}

	public void Delete(int index)
	{
		string filePath = GetFileSavePath() + index + ".dat";
		string imagePath = GetFileSavePath() + index + ".png";

		Godot.DirAccess.RemoveAbsolute(filePath);
		Godot.DirAccess.RemoveAbsolute(imagePath);
	}

	private void SaveDictionaryUintBool(Dictionary<uint, bool> toSave, Godot.FileAccess file)
	{
		file.Store16((ushort)toSave.Count);

		foreach(uint ui in toSave.Keys)
		{
			file.Store32(ui);

			ushort boolval = toSave[ui] == false ? (ushort)0 : (ushort)1;
			file.Store16(boolval);
		}
	}

	private void SaveDictionaryStringDouble(Dictionary<string, double> toSave, Godot.FileAccess file)
	{
		file.Store16((ushort)toSave.Count);

		foreach(string str in toSave.Keys)
		{
			file.StorePascalString(str);
			double d = toSave[str];
			file.StoreDouble(d);
		}
	}

	private void SaveDictionaryUlongBool(Dictionary<ulong, bool> toSave, Godot.FileAccess file)
	{
		file.Store16((ushort)toSave.Count);

		foreach(ulong ul in toSave.Keys)
		{
			file.Store64(ul);

			ushort boolval = toSave[ul] == false ? (ushort)0 : (ushort)1;
			file.Store16(boolval);
		}
	}

	private void SaveDictionaryVector3Bool(Dictionary<Vector3, bool> toSave, Godot.FileAccess file)
	{
		file.Store16((ushort)toSave.Count);

		foreach(Vector3 v3 in toSave.Keys)
		{
			file.StoreFloat(v3.X);
			file.StoreFloat(v3.Y);
			file.StoreFloat(v3.Z);

			ushort boolval = toSave[v3] == false ? (ushort)0 : (ushort)1;
			file.Store16(boolval);
		}
	}

	private void SaveDictionaryStringBool(Dictionary<string, bool> toSave, Godot.FileAccess file)
	{
		file.Store16((ushort)toSave.Count); //Store the count of elements so we know how much to load later on.
		foreach(string key in toSave.Keys)
		{
			file.StorePascalString(key);
			ushort toStore = toSave[key] ? (ushort)1 : (ushort)0;
			file.Store16(toStore); //Store the bool
		}
	}

	public string GetCurrentFileSavePath(){
		if(saveFileIndex == -1){
			return null;
		}

		Directory.CreateDirectory("./savedata");

		return $"./savedata/save_game{saveFileIndex}";
	}

	public string GetFileSavePath(){
		Directory.CreateDirectory("./savedata");
		return $"./savedata/save_game";
	}

	public string GetFileSaveDirectory()
	{
		Directory.CreateDirectory("./savedata");
		return $"./savedata/";
	}

	public void UpdateAllPickupCounts()
	{
		UpdateTotalCrownsCollected();
		UpdateTotalDonutsCollected();
		UpdateTotalPortalsCollected();
	}

	private void UpdateTotalCrownsCollected()
	{
		int totalCrownsCollected = 0;
		foreach(string key in GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.Keys)
		{
			if(!GameManager.Instance.saveManager.saveDataValues.levelsUnlocked[key]){
				continue;
			}

			int crownsCollectedInLevel = 0;
			if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(key))
			{
				foreach(uint id in GameManager.Instance.saveManager.saveDataValues.crownCollectedData[key].Keys)
				{
					if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData[key][id])
					{
						crownsCollectedInLevel += 1;
					}
				}
			}
			totalCrownsCollected += crownsCollectedInLevel;
		}

		//Steam achievements for getting different numbers of crowns.
		if(totalCrownsCollected >= 8)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("crown_count_8");
		}

		if(totalCrownsCollected >= 16)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("crown_count_16");
		}

		if(totalCrownsCollected >= 32)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("crown_count_32");
		}

		if(totalCrownsCollected >= 64)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("crown_count_64");
		}

		if(totalCrownsCollected >= 100)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("king_of_walking");
		}

		GameManager.Instance.crownCount = totalCrownsCollected;
	}

	public void UpdateTotalDonutsCollected()
	{
		int totalDonutsCollected = 0;
		foreach(string key in GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.Keys)
		{
			if(!GameManager.Instance.saveManager.saveDataValues.levelsUnlocked[key]){
				continue;
			}

			int donutsCollectedInLevel = 0;
			if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(key))
			{
				foreach(uint id in GameManager.Instance.saveManager.saveDataValues.donutCollectedData[key].Keys)
				{
					if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData[key][id])
					{
						donutsCollectedInLevel += 1;
					}
				}
			}
			totalDonutsCollected += donutsCollectedInLevel;
		}

		if(totalDonutsCollected >= 300)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("donut_count_300");
		}

		GameManager.Instance.donutCount = totalDonutsCollected;
	}

	public void UpdateTotalPortalsCollected()
	{

		int totalPortalsCollected = 0;
		foreach(string key in GameManager.Instance.saveManager.saveDataValues.levelsUnlocked.Keys)
		{
			if(!GameManager.Instance.saveManager.saveDataValues.levelsUnlocked[key]){
				continue;
			}

			int portalsDiscoveredInLevel = 0;
			if(GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(key))
			{
				foreach(uint id in GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[key].Keys)
				{
					if(GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[key][id])
					{
						portalsDiscoveredInLevel += 1;
					}
				}
			}
			totalPortalsCollected += portalsDiscoveredInLevel;
		}

		if(totalPortalsCollected >= 44)
		{
			GameManager.Instance.steamManager.UnlockSteamAchievement("all_portals");
		}

		GameManager.Instance.portalCount = totalPortalsCollected;
	}

	public struct SaveDataValues{
		public uint deathCount = 0;

		//If the player reaches any of the end portals or exits (i.e, the green and red portals plus the road tunnels) they have 'cleared' the level.
		public Dictionary<string, bool> levelsCleared = new Dictionary<string, bool>();

		//Generally if a player visits a level we mark it unlocked (true)
		public Dictionary<string, bool> levelsUnlocked = new Dictionary<string, bool>();
		public Dictionary<string, bool> endingsWon = new Dictionary<string, bool>();

		//Important as this keeps track of where the crowns have been collected so that we may replace them with fake crowns if the level is revisited.
		//If we don't implement something like this we end up with a situation where the player can leave and revisit a level many times to collect unlimited crowns.
		public Dictionary<string, Dictionary<uint, bool>> crownCollectedData = new Dictionary<string, Dictionary<uint, bool>>();
		public Dictionary<string, Dictionary<uint, bool>> donutCollectedData = new Dictionary<string, Dictionary<uint, bool>>();
		public Dictionary<string, Dictionary<uint, bool>> portalDiscoveredData = new Dictionary<string, Dictionary<uint, bool>>();

		public Dictionary<string, double> timeAttackLevelTimes = new Dictionary<string, double>();

        public SaveDataValues()
        {
			//Leave empty, vars are initialized in their definitions to avoid repeated code.
        }
    }
}
