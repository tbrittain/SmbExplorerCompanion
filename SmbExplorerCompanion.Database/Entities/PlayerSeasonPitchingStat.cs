namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSeasonPitchingStat
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public int Hits { get; set; }
    public int EarnedRuns { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public double InningsPitched { get; set; }
    public double? EarnedRunAverage { get; set; }
    public int TotalPitches { get; set; }
    public int Saves { get; set; }
    public int HitByPitch { get; set; }
    public int BattersFaced { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public int GamesFinished { get; set; }
    public int RunsAllowed { get; set; }
    public int WildPitches { get; set; }
    public double? BattingAverageAgainst { get; set; }
    public double? Fip { get; set; }
    public double? Whip { get; set; }
    public double? WinPercentage { get; set; }
    public double? OpponentObp { get; set; }
    public double? StrikeoutsPerNine { get; set; }
    public double? WalksPerNine { get; set; }
    public double? HitsPerNine { get; set; }
    public double? HomeRunsPerNine { get; set; }
    public double? PitchesPerInning { get; set; }
    public double? PitcherPerGame { get; set; }
    public double? EraMinus { get; set; }
    public double? FipMinus { get; set; }
    public bool IsRegularSeason { get; set; } = true;
}