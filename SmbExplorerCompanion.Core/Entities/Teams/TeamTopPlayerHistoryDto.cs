using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamTopPlayerHistoryDto
{
    public int PlayerId { get; set; }
    public int NumSeasonsWithTeam { get; set; }
    public List<int> SeasonNumbers { get; set; } = new();
    public bool IsPitcher { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerPosition { get; set; } = string.Empty;
    
    public double AverageOpsPlus { get; set; }
    public double AverageEraMinus { get; set; }
    /// <summary>
    /// This will be a composite of the Player's average OPS OR a Pitcher's ERA- multiplied by the
    /// number of games they played with the team (roughly).
    /// In bRef, this ordering on the Team page is by bWAR
    /// </summary>
    public double WeightedOpsPlusOrEraMinus { get; set; }
    public List<PlayerAwardBaseDto> Awards { get; set; } = new();
}