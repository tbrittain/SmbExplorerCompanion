using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class PlayerLeaderSummary : PlayerBase
{
    public string StatName { get; set; } = string.Empty;
    public double StatValue { get; set; }
}