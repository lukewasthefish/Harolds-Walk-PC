using Godot;
using System;

/// <summary>
/// Tracks portals the player has visited in each level
/// </summary>
/// TODO : In these pickup exteneded classes we have a lot of repeating code and it gets hard to manage... restructure pls
public partial class SecretExitPickup : Pickup
{
    public override void Initialize()
    {
		if(GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(GameManager.Instance.levelManager.currentScene) && GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[GameManager.Instance.levelManager.currentScene].ContainsKey(GetUniqueSaveID()))
		{
			if(GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()])
			{
				this.Visible = false;
			}
		} else {
			if(!GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(GameManager.Instance.levelManager.currentScene))
			{
				GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.Add(GameManager.Instance.levelManager.currentScene, new System.Collections.Generic.Dictionary<uint, bool>());
			}
			GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()] = false; //Not yet collected by player

			this.Visible = true;
		}
    }

    public override void CollectAction(){
		if(!GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.ContainsKey(GameManager.Instance.levelManager.currentScene)){
			GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData.Add(GameManager.Instance.levelManager.currentScene, new System.Collections.Generic.Dictionary<uint, bool>());
		}

		GameManager.Instance.saveManager.saveDataValues.portalDiscoveredData[GameManager.Instance.levelManager.currentScene][GetUniqueSaveID()] = true;
	} 
}
