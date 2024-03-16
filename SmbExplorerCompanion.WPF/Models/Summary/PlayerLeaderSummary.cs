using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class PlayerLeaderSummary : PlayerBase
{
    public string StatName { get; set; } = string.Empty;
    public double StatValue { get; set; }
}

public static class PlayerLeaderSummaryExtensions
{
    public static PlayerLeaderSummary FromCore(this PlayerLeaderSummaryDto x)
    {
        return new PlayerLeaderSummary
        {
            PlayerId = x.PlayerId,
            PlayerName = x.PlayerName,
            StatName = x.StatName,
            StatValue = x.StatValue
        };
    }
}