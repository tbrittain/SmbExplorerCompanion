using System.Collections.ObjectModel;
using System.Windows;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class FranchiseSummary
{
    public int NumPlayers { get; set; }
    public int NumSeasons { get; set; }
    public int MostRecentSeasonNumber { get; set; }
    public int NumHallOfFamers { get; set; }

    public int? MostRecentChampionTeamId { get; set; }
    public string? MostRecentChampionTeamName { get; set; }
    
    public Visibility MostRecentChampionVisibility => MostRecentChampionTeamId.HasValue ? Visibility.Visible : Visibility.Collapsed;

    public PlayerLeaderSummary TopHomeRuns { get; set; } = new();
    public PlayerLeaderSummary TopHits { get; set; } = new();
    public PlayerLeaderSummary TopRunsBattedIn { get; set; } = new();
    public PlayerLeaderSummary TopWins { get; set; } = new();
    public PlayerLeaderSummary TopSaves { get; set; } = new();
    public PlayerLeaderSummary TopStrikeouts { get; set; } = new();
    
    // We will use this in a similar way to how BBREF shows a random selection of players on their home page
    public ObservableCollection<PlayerBase> RandomPlayers { get; set; } = new();

}