using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Extensions;

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

public static class TeamOverviewExtensions
{
    public static TeamOverview FromCore(this TeamOverviewDto x)
    {
        return new TeamOverview
        {
            NumSeasons = x.NumSeasons,
            NumWins = x.NumWins,
            NumLosses = x.NumLosses,
            WinPercentage = x.WinPercentage,
            NumPlayoffAppearances = x.NumPlayoffAppearances,
            NumDivisionsWon = x.NumDivisionsWon,
            NumConferenceTitles = x.NumConferenceTitles,
            NumChampionships = x.NumChampionships,
            TeamHistory = x.TeamHistory.Select(y => y.FromCore()).ToObservableCollection(),
            TopPlayers = x.TopPlayers.Select(y => y.FromCore()).ToObservableCollection()
        };
    }
}