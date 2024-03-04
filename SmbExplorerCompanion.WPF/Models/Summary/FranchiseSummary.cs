using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.WPF.Extensions;
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

    public int? MostRecentMvpPlayerId { get; set; }
    public string? MostRecentMvpPlayerName { get; set; }

    public int? MostRecentCyYoungPlayerId { get; set; }
    public string? MostRecentCyYoungPlayerName { get; set; }

    public bool HasMostRecentChampion => MostRecentChampionTeamId.HasValue;
    public bool HasMostRecentMvp => MostRecentMvpPlayerId.HasValue;
    public bool HasMostRecentCyYoung => MostRecentCyYoungPlayerId.HasValue;

    public PlayerLeaderSummary TopHomeRuns { get; set; } = new();
    public PlayerLeaderSummary TopHits { get; set; } = new();
    public PlayerLeaderSummary TopRunsBattedIn { get; set; } = new();
    public PlayerLeaderSummary TopWins { get; set; } = new();
    public PlayerLeaderSummary TopSaves { get; set; } = new();
    public PlayerLeaderSummary TopStrikeouts { get; set; } = new();

    public ObservableCollection<PlayerCareerBase> CurrentGreats { get; set; } = new();
}