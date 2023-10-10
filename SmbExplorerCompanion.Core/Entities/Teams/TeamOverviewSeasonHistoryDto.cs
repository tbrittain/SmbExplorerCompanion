namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamOverviewSeasonHistoryDto
{
    public int SeasonTeamId { get; set; }
    public int SeasonNumber { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string ConferenceName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double GamesBehind { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public int? PlayoffSeed { get; set; }
    public bool WonConference { get; set; }
    public bool WonChampionship { get; set; }
}