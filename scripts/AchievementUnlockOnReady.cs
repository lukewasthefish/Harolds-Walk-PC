using Godot;

/// <summary>
/// Unlocks a Steam achievment specified by a string when this Node has its _Ready() override function called.
/// </summary>
public partial class AchievementUnlockOnReady : Node
{
	[Export]
	private string achievmentNameInAPI = "ending1"; //DON'T change the default value of this

	[Export]
	private bool requireZeroDeaths = false;

	public override void _Ready()
	{
		if(requireZeroDeaths && GameManager.Instance.saveManager.saveDataValues.deathCount != 0)
		{
			return;
		}

		GameManager.Instance.steamManager.UnlockSteamAchievement(achievmentNameInAPI);
	}
}
