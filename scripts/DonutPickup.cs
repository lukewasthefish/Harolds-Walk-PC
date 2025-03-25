using Godot;
using System;

public partial class DonutPickup : Pickup
{
	[Export]
	private GpuParticles3D particles3D;
	public override void _Ready()
	{
		base._Ready();
	}

	public override void Initialize()
	{
		if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene) && GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene].ContainsKey(GetUniqueSaveID()))
		{
			if(GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()])
			{
				PackedScene fakeDonutResource = (PackedScene)GD.Load("res://Scenes/fakeDonut.tscn");
				Node fd = fakeDonutResource.Instantiate();

				this.Visible = false;
			}
		} else {
			if(!GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene)){
				GameManager.Instance.saveManager.saveDataValues.donutCollectedData.Add(GameManager.Instance.levelManager.currentScene, new System.Collections.Generic.Dictionary<uint, bool>());
			}
			GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()] = false; //Not yet collected by player

			this.Visible = true;
		}
	}

	public override void CollectAction()
	{
		particles3D.Emitting = true;

		particles3D.Visible = true;

		if(!(Extensions.IsValid(particles3D)))
		{
			return;
		}
		particles3D.Emitting = true;

		if(!GameManager.Instance.saveManager.saveDataValues.donutCollectedData.ContainsKey(GameManager.Instance.levelManager.currentScene)){
			GameManager.Instance.saveManager.saveDataValues.donutCollectedData.Add(GameManager.Instance.levelManager.currentScene, new System.Collections.Generic.Dictionary<uint, bool>());
		}
		GameManager.Instance.saveManager.saveDataValues.donutCollectedData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()] = true;

		GameManager.Instance.playermovement.ChompSound();

		GameManager.Instance.playerUIManager.UpdateGuiCounters();
	}
}
