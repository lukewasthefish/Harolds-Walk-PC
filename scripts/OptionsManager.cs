using Godot;
using System;
using System.IO;

public partial class OptionsManager : Node
{
	public Godot.Collections.Dictionary options;

	public override void _Ready()
	{
		GameManager.Instance.optionsManager ??= this;

		if(this != GameManager.Instance.optionsManager)
		{
			return;
		}

		options = LoadOptionsFromJson();
		ApplyOptions();
	}

	public void SetOptionsToDefaults()
	{
		options = DefaultOptions();
	}

	private Godot.Collections.Dictionary DefaultOptions()
	{
		Godot.Collections.Dictionary defaults = new Godot.Collections.Dictionary
        {
            { "musicEnabled", true },
            { "soundEnabled", true },
            { "musicScale", 1.0f },
            { "soundScale", 1.0f },
            { "fullscreen", true },
            { "vsync", false },
            { "scaling3D", 1.0f },
            { "fps", 60 },
			{ "cameraRotateSpeedScale", 1.0f},
			{ "cameraRotateWithPlayer", true}
        };

		return defaults;
	}

	public void SaveOptionsAsJson(Godot.Collections.Dictionary optionsToSave)
	{
		Directory.CreateDirectory("./settings");

		string path = "./settings/settings.json";
		string data = Json.Stringify(optionsToSave);

		try
		{
			File.WriteAllText(path, data);
		} catch (System.Exception e)
		{
			GD.Print(e);	
		}
	}

	public Godot.Collections.Dictionary LoadOptionsFromJson()
	{
		string data = null;

		string path = "./settings/settings.json";

		if(!File.Exists(path))
		{
			return DefaultOptions();
		}

		try 
		{
			data = File.ReadAllText(path);
		} catch (System.Exception e)
		{
			GD.Print(e);
		}

		Json jsonLoader = new Json();

		Error error = jsonLoader.Parse(data);

		if(error != Error.Ok)
		{
			GD.Print(error);
			return DefaultOptions();
		}

		Godot.Collections.Dictionary loadedData = (Godot.Collections.Dictionary)jsonLoader.Data;

		//Do this so that if we update the game with new options we will just accept the defaults and not break everything else in the game.
		//This also prevents issues that can happen when players tamper with the options data on their own.
		if(loadedData.Count != DefaultOptions().Count)
		{
			//TODO : Find keys in existing options file that DO match and transfer those over while importing the defaults for new options.
			return DefaultOptions();
		}

		return loadedData;
	}

	public void ApplyOptions()
	{
		if(options.ContainsKey("scaling3D"))
		GetViewport().Scaling3DScale = (float)options["scaling3D"];

		if(options.ContainsKey("fullscreen") && (bool)options["fullscreen"])
		{
			if(DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen)
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		} else 
		{
			if(DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Windowed)
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}

		if(options.ContainsKey("fps"))
		Engine.MaxFps = (int)options["fps"];

		if(options.ContainsKey("vsync") && !(bool)options["vsync"])
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
		} else 
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
		}
	}
}
