using Godot;
using System;
using System.Collections.Generic;

public partial class SoundManager : Node
{
	private List<AudioStreamPlayer> activeSounds = new List<AudioStreamPlayer>();

    public override void _Ready()
    {
        base._Ready();

		GameManager.Instance.soundManager ??= this;
    }

    public void PlaySound(AudioStream audioStream, float randomPitch = 0.0f)
	{
		if(!(bool)GameManager.Instance.optionsManager.options["soundEnabled"])
		{
			return;
		}

		AudioStreamPlayer audioStreamPlayer = new AudioStreamPlayer();
		activeSounds.Add(audioStreamPlayer);
		this.AddChild(audioStreamPlayer);

		audioStreamPlayer.Stream = audioStream;
		audioStreamPlayer.VolumeDb = Mathf.Lerp(-60.0f, audioStreamPlayer.VolumeDb, (float)GameManager.Instance.optionsManager.options["soundScale"]);
		audioStreamPlayer.PitchScale = (float)GD.RandRange(1.0f - randomPitch, 1.0f + randomPitch);
		audioStreamPlayer.Play();
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

		List<AudioStreamPlayer> toRemove = new List<AudioStreamPlayer>();
		foreach(AudioStreamPlayer audioStreamPlayer in activeSounds)
		{
			if(Extensions.IsValid(audioStreamPlayer) && !audioStreamPlayer.Playing)
			{
				toRemove.Add(audioStreamPlayer);
			}
		}

		foreach(AudioStreamPlayer audioStreamPlayer in toRemove)
		{
			if(Extensions.IsValid(audioStreamPlayer)){
				audioStreamPlayer.Stop();
				audioStreamPlayer.QueueFree();
			}
		}
    }
}
