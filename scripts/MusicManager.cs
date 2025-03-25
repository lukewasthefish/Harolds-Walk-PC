using Godot;
using System;

public partial class MusicManager : AudioStreamPlayer
{

	private AudioStreamOggVorbis previousSong;
	private AudioStreamOggVorbis currentSong;

	private float baseMusicVolume;

	private const float MIN_DB = -60.0f;

	public override void _Ready()
	{
		GameManager.Instance.musicManager ??= this;

		baseMusicVolume = this.VolumeDb;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if(!(bool)GameManager.Instance.optionsManager.options["musicEnabled"])
		{
			this.VolumeDb = MIN_DB;
		} else 
		{
			this.VolumeDb = Mathf.Lerp(this.VolumeDb, Mathf.Lerp(MIN_DB, baseMusicVolume, (float)GameManager.Instance.optionsManager.options["musicScale"]), 2f * (float)delta);
		}
	}

	public void UpdateCurrentSong(AudioStreamOggVorbis newSong)
	{
		if(newSong == null)
		{
			Stop();
			return;
		}

		//Don't change the song if the player dies and restarts a level OR the next level continues with the same song.
		if(newSong.Equals(this.currentSong))
		{
			return;
		}

		previousSong = this.currentSong;

		this.currentSong = newSong;

		this.Stream = this.currentSong;
		Stop();

		this.VolumeDb = MIN_DB;
		Play();
	}
}
