namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamOverviewHistoryDto
{
    public int SeasonTeamId { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNumber { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int GamesBehind { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public int? PlayoffSeed { get; set; }
    public bool WonConference { get; set; }
    public bool WonChampionship { get; set; }
}