using System.Collections.ObjectModel;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamTopPlayerHistory
{
    public int PlayerId { get; set; }
    public int NumSeasonsWithTeam { get; set; }
    public bool IsPitcher { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerPosition { get; set; } = string.Empty;
    public double AverageOpsPlus { get; set; }
    public double AverageEraMinus { get; set; }
    public double AverageOpsPlusOrEraMinus => IsPitcher ? AverageEraMinus : AverageOpsPlus;
    public double WeightedOpsPlusOrEraMinus { get; set; }
    public ObservableCollection<PlayerAwardBase> Awards { get; set; } = new();
    public string? DisplayAwards => Awards.GetFormattedAwards(isSeason: false);
}