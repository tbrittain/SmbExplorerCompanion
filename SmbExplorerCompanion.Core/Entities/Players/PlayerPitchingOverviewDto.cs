namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerPitchingOverviewDto : PlayerPitchingSeasonDto
{
    public int Age { get; set; }
    public double WinPercentage => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) : 0;
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