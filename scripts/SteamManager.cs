using Godot;
using GodotSteam;

public partial class SteamManager : Node
{
	private const uint AppId = 1009360;


	public override void _EnterTree()
	{
		GameManager.Instance.steamManager ??= this;

		OS.SetEnvironment("SteamAppId", AppId.ToString());
		OS.SetEnvironment("SteamGameId", AppId.ToString());
	}

	public override void _Ready()
	{
		GameManager.Instance.steamManager ??= this;
		Steam.SteamInit();

		var isSteamRunning = Steam.IsSteamRunning();
		if (!isSteamRunning)
		{
			//GD.Print("steam not in use.");
			return;
		}

		//GD.Print("steam running.");
		
		//var steamId = Steam.GetSteamID();
		//var name = Steam.GetFriendPersonaName(steamId);	
	}

	/// <summary>
	/// If the achievement signified by 'achievementName' in the Steam API settigns is not unlocked, achieve it.
	/// Make sure that any calls to this function can be repeated if the player has already achieved the achievement.
	/// i.e, when checking crown count related achievements we check all previous crown counts before checking the current one to ensure all the crown count achievements can happen.
	/// </summary>
	/// <param name="achievementName"></param>
	public void UnlockSteamAchievement(string achievementName)
	{
		if(!Steam.IsSteamRunning())
		{
			return;
		}

		var achievedStatus = Steam.GetAchievement(achievementName);

		if((bool)achievedStatus["achieved"] == false)
		{
			Steam.SetAchievement(achievementName);
			Steam.StoreStats();
		}

	}
}
