namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamTopPlayerHistory
{
    public int PlayerId { get; set; }
    public int NumSeasonsWithTeam { get; set; }
    public double AverageOpsPlus { get; set; }
    public double WeightedOpsPlus { get; set; }
}