namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerFieldingRanking : PlayerBase
{
    public string TeamNames { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public int SeasonId { get; set; }
    public int Speed { get; set; }
    public int Fielding { get; set; }
    public int? Arm { get; set; }
    public int? PlateAppearances { get; set; }
    public double? InningsPitched { get; set; }
    public int Errors { get; set; }
    public int PassedBalls { get; set; }
    public double WeightedFieldingRanking { get; set; }
}