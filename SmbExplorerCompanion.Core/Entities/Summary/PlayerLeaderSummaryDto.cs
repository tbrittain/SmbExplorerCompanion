using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Entities.Summary;

public class PlayerLeaderSummaryDto : PlayerBaseDto
{
    public string StatName { get; set; } = string.Empty;
    public double StatValue { get; set; }
}