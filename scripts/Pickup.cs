using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Pickup : Area3D
{
	public bool collected = false;

	private uint uniqueSaveID;

    public override void _Ready()
    {
        base._Ready();

		if(GameManager.Instance.gameMode == GameManager.GameMode.TIME_ATTACK)
		{
			this.collected = true;
			this.Visible = false;
			return;
		}

		uniqueSaveID = GameManager.Instance.saveManager.GetNewSaveID();

		GameManager.Instance.pickupsInScene.Add(this);

		//GD.Print(uniqueSaveID + " was assigned to node " + this.Name);

		Initialize();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

		//We want to always be looking at world up. This is important for rotating stages 'see Afterparty'
		this.LookAt(this.GlobalPosition + Vector3.Forward);
    }

    private void _on_area_entered(Area3D area)
	{
		if(Visible && !collected && area.IsInGroup("Player"))
		{
			this.Visible = false;
			CollectAction();
			GameManager.Instance.saveManager.UpdateAllPickupCounts();

			collected = true;
		}
	}

	protected virtual uint GetUniqueSaveID()
	{
		return uniqueSaveID;
	}

	public abstract void CollectAction();

	public abstract void Initialize();
}
