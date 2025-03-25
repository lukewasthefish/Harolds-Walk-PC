using Godot;
using System;

public partial class CharacterSwitcherCollider : Area3D
{
	public override void _Ready()
	{
		this.AreaEntered += SwitchCharacter;
	}

    private void SwitchCharacter(Area3D area)
	{
		if(!area.IsInGroup("Player"))
		{
			return;
		}

		switch(GameManager.Instance.selectedCharacter)
		{
			case GameManager.SelectedCharacter.HAROLD:
			GameManager.Instance.selectedCharacter = GameManager.SelectedCharacter.UMA;
			break;

			case GameManager.SelectedCharacter.UMA:
			GameManager.Instance.selectedCharacter = GameManager.SelectedCharacter.HAROLD;
			break;
		}
	}
}
