using Godot;
using System;

//NOTE : We disabled these for now as they were interfering with the uniqueObjectID stuff and breaking save data
public partial class fakePickup : Pickup
{
	public Vector3 destinationPosition;

	public override void CollectAction()
	{
	}

	public override void _Ready()
	{
		base._Ready();

		this.GlobalPosition = destinationPosition;
	}

	public override void Initialize()
	{
		throw new NotImplementedException();
	}
}
