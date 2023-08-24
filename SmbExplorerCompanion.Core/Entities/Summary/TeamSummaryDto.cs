namespace SmbExplorerCompanion.Core.Entities.Summary;

public class TeamSummaryDto
{
    public int Id { get; set; }
    public int SeasonTeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int? PlayoffSeed { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public bool IsDivisionChampion { get; set; }
    public bool IsConferenceChampion { get; set; }
    public bool IsChampion { get; set; }
}