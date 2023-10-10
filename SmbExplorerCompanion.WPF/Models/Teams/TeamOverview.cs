using System.Collections.ObjectModel;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamOverview : TeamBase
{
    public ObservableCollection<TeamOverviewSeasonHistory> TeamHistory { get; set; } = new();
    public ObservableCollection<TeamTopPlayerHistory> TopPlayers { get; set; } = new();
    public int NumSeasons { get; set; }
    public int NumWins { get; set; }
    public int NumLosses { get; set; }
    public double WinPercentage { get; set; }
    public int NumPlayoffAppearances { get; set; }
    public int NumDivisionsWon { get; set; }
    public int NumConferenceTitles { get; set; }
    public int NumChampionships { get; set; }

    public string TeamRecord => $"{NumWins}-{NumLosses}, {WinPercentage:F3} W-L%";
}