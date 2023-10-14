namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerKpiPercentileDto
{
    // Batters
    public double Hits { get; set; }
    public double HomeRuns { get; set; }
    public double BattingAverage { get; set; }
    public double StolenBases { get; set; }
    public double BatterStrikeouts { get; set; }
    public double Obp { get; set; }
    public double Slg { get; set; }

    // Pitchers
    public double Wins { get; set; }
    public double Era { get; set; }
    public double Whip { get; set; }
    public double InningsPitched { get; set; }
    public double PitcherStrikeoutsPerNine { get; set; }
    public double PitcherWalksPerNine { get; set; }
    public double PitcherStrikeoutToWalkRatio { get; set; }
}