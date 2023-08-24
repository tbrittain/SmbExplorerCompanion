namespace SmbExplorerCompanion.WPF.Models.Summary;

public class TeamSummary
{
    public int Id { get; set; }
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