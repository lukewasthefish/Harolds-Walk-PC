using Godot;
using System;

public partial class MusicPlayer : Node
{
	[Export]
	AudioStreamOggVorbis song;

	public override void _Ready()
	{
		GameManager.Instance.musicManager.UpdateCurrentSong(song);
	}
}
