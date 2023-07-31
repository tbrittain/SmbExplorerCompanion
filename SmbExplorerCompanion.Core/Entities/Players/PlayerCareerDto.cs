namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerCareerDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsPitcher { get; set; }

    public int TotalSalary { get; set; }
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string? PitcherRole { get; set; }
    public string Chemistry { get; set; } = string.Empty;
    public int StartSeasonNumber { get; set; }
    public int EndSeasonNumber { get; set; }
    public bool IsRetired { get; set; }
    public int NumSeasons { get; set; }

    // Batting stats, applicable to both pitchers and batters
    public int AtBats { get; set; }
    public int Hits { get; set; }
    public int Singles { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public double BattingAverage { get; set; }
    public int Runs { get; set; }
    public int RunsBattedIn { get; set; }
    public int StolenBases { get; set; }
    public int HitByPitch { get; set; }
    public int SacrificeHits { get; set; }
    public int SacrificeFlies { get; set; }
    public double Obp { get; set; }
    public double Slg { get; set; }
    public double Ops { get; set; }
    public double OpsPlus { get; set; }
    public int Errors { get; set; }

    // Pitching stats, may apply to position players who have pitched
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int EarnedRuns { get; set; }
    public int TotalPitches { get; set; }
    public double Era { get; set; }
    public double Fip { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public double Whip { get; set; }
    public double EraMinus { get; set; }
    public double FipMinus { get; set; }

    /// <summary>
    ///  We can kind of think of this as a proxy for WAR, but it's not quite the same
    /// </summary>
    public double WeightedOpsPlusOrEraMinus { get; set; }
}