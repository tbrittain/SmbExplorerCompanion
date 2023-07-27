namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamOverviewDto
{
    public int TeamId { get; set; }
    public string CurrentTeamName { get; set; } = string.Empty;
    public List<TeamOverviewSeasonHistoryDto> TeamHistory { get; set; } = new();
    public List<TeamTopPlayerHistoryDto> TopPlayers { get; set; } = new();
    public int NumSeasons { get; set; }
    public int NumWins { get; set; }
    public int NumLosses { get; set; }
    public double WinPercentage => NumSeasons == 0 ? 0 : (double) NumWins / (NumLosses + NumWins);
    public int NumPlayoffAppearances { get; set; }
    public int NumDivisionsWon { get; set; }
    public int NumConferenceTitles { get; set; }
    public int NumChampionships { get; set; }
}