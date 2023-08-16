namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerPitchingOverview : PlayerSeasonPitching
{
    public int Age { get; set; }
    public double WinPercentage { get; set; }
    public double Era { get; set; }
    public int Games { get; set; }
    public int GamesFinished { get; set; }
    public int BattersFaced { get; set; }
    public double HitsPerNine { get; set; }
    public double HomeRunsPerNine { get; set; }
    public double WalksPerNine { get; set; }
    public double StrikeoutsPerNine { get; set; }
    public double StrikeoutToWalkRatio { get; set; }
    public int Salary { get; set; }
    public string Traits { get; set; } = string.Empty;
    public string PitchTypes { get; set; } = string.Empty;
}