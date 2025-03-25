using Godot;
using System;

public partial class PlayAnimation : Node
{
	[Export]
	private AnimationPlayer animationPlayer;

	[Export]
	private string animationName;

	public override void _Process(double delta)
	{
		animationPlayer.Play(animationName);
	}
}
