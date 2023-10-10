using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Entities.Summary;

public class FranchiseSummaryDto
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

    public PlayerLeaderSummaryDto TopHomeRuns { get; set; } = new();
    public PlayerLeaderSummaryDto TopHits { get; set; } = new();
    public PlayerLeaderSummaryDto TopRunsBattedIn { get; set; } = new();
    public PlayerLeaderSummaryDto TopWins { get; set; } = new();
    public PlayerLeaderSummaryDto TopSaves { get; set; } = new();
    public PlayerLeaderSummaryDto TopStrikeouts { get; set; } = new();

    public List<PlayerCareerBaseDto> CurrentGreats { get; set; } = new();
}