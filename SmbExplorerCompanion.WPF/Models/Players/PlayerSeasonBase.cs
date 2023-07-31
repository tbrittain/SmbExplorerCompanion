namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerSeasonBase : PlayerBase
{
    public int SeasonNumber { get; set; }
    public string TeamNames { get; set; } = string.Empty;
}