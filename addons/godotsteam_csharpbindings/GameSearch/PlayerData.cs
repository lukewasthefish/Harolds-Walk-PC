namespace GodotSteam;

public class PlayerData
{
    public ulong PlayerId { get; set; }
    public ulong LobbyId { get; set; }
    public int PlayerAcceptState { get; set; } 
    public int PlayerIndex { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalPlayersAcceptedGame { get; set; }
    public int SuggestedTeamIndex { get; set; }
    public ulong UniqueGameId { get; set; }
}