using Godot;
using System;
using System.IO;

public partial class OptionsMenu : Node
{
	//Audio
	[Export]
	private CheckButton musicEnabledButton;
	[Export]
	private CheckButton soundEnabledButton;

	[Export]
	private HSlider musicScaler;
	[Export]
	private HSlider soundScaler;
	[Export]
	private HSlider cameraRotationSpeedScaler;

	//Screen
	[Export]
	private CheckButton fullscreenToggleButton;

	//Graphics
	[Export]
	private OptionButton resolutionScalingButton;
	[Export]
	private OptionButton fpsButton;
	[Export]
	private CheckButton vsyncButton;
	[Export]
	private CheckButton cameraRotateWithPlayerButton;

	//Controls
	//TODO

	public override void _Ready()
	{
		GameManager.Instance.optionsManager.options = GameManager.Instance.optionsManager.LoadOptionsFromJson();
		SetupUIStateFromLoadedOptions();
		SetupSignals();
	}

	private void SetupUIStateFromLoadedOptions()
	{
		//Check buttons
		fullscreenToggleButton.SetPressedNoSignal((bool)GameManager.Instance.optionsManager.options["fullscreen"]);
		vsyncButton.SetPressedNoSignal((bool)GameManager.Instance.optionsManager.options["vsync"]);
		cameraRotateWithPlayerButton.SetPressedNoSignal((bool)GameManager.Instance.optionsManager.options["cameraRotateWithPlayer"]);
		musicEnabledButton.SetPressedNoSignal((bool)GameManager.Instance.optionsManager.options["musicEnabled"]);
		soundEnabledButton.SetPressedNoSignal((bool)GameManager.Instance.optionsManager.options["soundEnabled"]);

		//HSliders
		musicScaler.SetValueNoSignal((float)GameManager.Instance.optionsManager.options["musicScale"]);
		//soundScaler.SetValueNoSignal(GameManager.Instance.optionsManager.options.soundScale);
		cameraRotationSpeedScaler.SetValueNoSignal((float)GameManager.Instance.optionsManager.options["cameraRotateSpeedScale"]);

		//Options buttons
		//Scaling3D
		float res = (float)GameManager.Instance.optionsManager.options["scaling3D"];

		for(int i = 0; i < resolutionScalingButton.ItemCount; i++)
		{
			string txt = resolutionScalingButton.GetItemText(i);

			if(float.TryParse(txt, out float f) && f == (float)GameManager.Instance.optionsManager.options["scaling3D"])
			{
				resolutionScalingButton.Select(i);
				break;
			}
		}

		fpsButton.Select(4); //selects 'unlimited' fps. This is the default.

		//FPS
		int fps = (int)GameManager.Instance.optionsManager.options["fps"];

		for(int i = 0; i < fpsButton.ItemCount; i++)
		{
			string txt = fpsButton.GetItemText(i);

			if(Int32.TryParse(txt, out int num) && num == (int)GameManager.Instance.optionsManager.options["fps"])
			{
				fpsButton.Select(i);
				break;
			}
		}

	}

	private void SetupSignals()
	{
		fullscreenToggleButton.Toggled += FullScreenToggled;
		vsyncButton.Toggled += VSYNCChagned;
		cameraRotateWithPlayerButton.Toggled += CameraRotateWithPlayerChanged;
		musicEnabledButton.Toggled += MusicEnabledToggled;
		soundEnabledButton.Toggled += SoundEnabledToggled;
		musicScaler.ValueChanged += MusicScaleChanged;
		cameraRotationSpeedScaler.ValueChanged += CameraRotationSpeedChanged;
		fpsButton.ItemSelected += FPSChanged;
		resolutionScalingButton.ItemSelected += ResolutionScalingChanged;
	}

	private void SoundScaleChanged(double value)
	{
		GameManager.Instance.optionsManager.options["soundScale"] = (float)value;
	}

	private void MusicScaleChanged(double value)
	{
		GameManager.Instance.optionsManager.options["musicScale"] = (float)value;
	}

	private void CameraRotationSpeedChanged(double value)
	{
		GameManager.Instance.optionsManager.options["cameraRotateSpeedScale"] = (float)value;
	}

	private void ResolutionScalingChanged(long index)
	{
		string resString = resolutionScalingButton.GetItemText((int)index);

		float resFloat = float.Parse(resString);

		GameManager.Instance.optionsManager.options["scaling3D"] = resFloat;
	}

	private void FullScreenToggled(bool toggledOn)
	{
		GameManager.Instance.optionsManager.options["fullscreen"] = toggledOn;
	}

	private void FPSChanged(long index)
	{
		string fpsString = fpsButton.GetItemText((int)index);

		if(fpsString == "unlimited (disables vsync)")
		{
			GameManager.Instance.optionsManager.options["vsync"] = false;
			GameManager.Instance.optionsManager.options["fps"] = 500;
			return;
		}

		int fpsInt = Int32.Parse(fpsString);

		GameManager.Instance.optionsManager.options["fps"] = fpsInt;
	}

	private void VSYNCChagned(bool toggledOn)
	{
		GameManager.Instance.optionsManager.options["vsync"] = toggledOn;
	}

	private void CameraRotateWithPlayerChanged(bool toggledOn)
	{
		GameManager.Instance.optionsManager.options["cameraRotateWithPlayer"] = toggledOn;
	}

	private void MusicEnabledToggled(bool toggledOn)
	{
		GameManager.Instance.optionsManager.options["musicEnabled"] = toggledOn;
	}

	private void SoundEnabledToggled(bool toggledOn)
	{
		GameManager.Instance.optionsManager.options["soundEnabled"] = toggledOn;
	}

	public void SaveOptionsAsJson()
	{
		Directory.CreateDirectory("./settings");

		string path = "./settings/";
		string filename  = "settings.json";
		string data = "uwu";

		try
		{
			File.WriteAllText(path + filename, data);
		} catch (System.Exception e)
		{
			GD.Print(e);	
		}
	}

	private void RevertOptionsToDefault()
	{
		GameManager.Instance.optionsManager.SetOptionsToDefaults();
		SetupUIStateFromLoadedOptions();
	}

	private void SaveAndApplyOptions()
	{
		GameManager.Instance.optionsManager.SaveOptionsAsJson(GameManager.Instance.optionsManager.options);
		GameManager.Instance.optionsManager.ApplyOptions();
	}
}
