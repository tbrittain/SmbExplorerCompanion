namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamTopPlayerHistoryDto
{
    public int PlayerId { get; set; }
    public int NumSeasonsWithTeam { get; set; }
    public double AverageOpsPlus { get; set; }
    /// <summary>
    /// This will be a composite of the Player's average OPS multiplied by the number of games they played with the team (roughly).
    /// In bRef, this ordering on the Team page is by bWAR
    /// </summary>
    public double WeightedOpsPlus { get; set; }
}