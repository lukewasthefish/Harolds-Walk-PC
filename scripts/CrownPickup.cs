using Godot;
using System;

public partial class CrownPickup : Pickup
{
	[Export]
	private GpuParticles3D particles3D;

	[Export]
	private AudioStream crownCollectedAudioStream;

	public override void Initialize()
	{
		if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene) && GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene].ContainsKey(GetUniqueSaveID()))
		{
			if(GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()])
			{
				this.Visible = false;
			}
		} else
		{
			if(!GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene)){
				GameManager.Instance.saveManager.saveDataValues.crownCollectedData.Add(GameManager.Instance.levelManager.currentScene, new System.Collections.Generic.Dictionary<uint, bool>());
			}

			GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()] = false; //Not yet collected by player
			this.Visible =  true;
		}
	}

	public override void CollectAction()
	{
		particles3D.Emitting = true;
		particles3D.Visible = true;

		if(!GameManager.Instance.saveManager.saveDataValues.crownCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene)){
			GameManager.Instance.saveManager.saveDataValues.crownCollectedData.Add(GameManager.Instance.levelManager.currentScene, new System.Collections.Generic.Dictionary<uint, bool>());
		}
		GameManager.Instance.saveManager.saveDataValues.crownCollectedData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()] = true;

		GameManager.Instance.soundManager.PlaySound(crownCollectedAudioStream);

		GameManager.Instance.playerUIManager.UpdateGuiCounters();
	}
}
