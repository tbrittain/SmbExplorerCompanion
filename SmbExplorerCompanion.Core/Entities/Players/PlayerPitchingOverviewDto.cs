namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerPitchingOverviewDto
{
    public int SeasonNumber { get; set; }
    public int Age { get; set; }
    public string TeamNames { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinPercentage => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) : 0;
    public double Era { get; set; }
    public int Games { get; set; }
    public int GamesStarted { get; set; }
    public int GamesFinished { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Hits { get; set; }
    public int EarnedRuns { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public int HitByPitch { get; set; }
    public int BattersFaced { get; set; }
    public double EraMinus { get; set; }
    public double Fip { get; set; }
    public double FipMinus { get; set; }
    public double Whip { get; set; }
    public double HitsPerNine { get; set; }
    public double HomeRunsPerNine { get; set; }
    public double WalksPerNine { get; set; }
    public double StrikeoutsPerNine { get; set; }
    public double StrikeoutToWalkRatio { get; set; }
    public int Salary { get; set; }
    public string Traits { get; set; } = string.Empty;
    public string PitchTypes { get; set; } = string.Empty;
}