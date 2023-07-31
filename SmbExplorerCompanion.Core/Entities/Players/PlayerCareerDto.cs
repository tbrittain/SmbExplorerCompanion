namespace SmbExplorerCompanion.Core.Entities.Players;

// TODO: May want to break this up into separate DTOs for pitchers and batters?
public class PlayerCareerDto : PlayerBaseDto
{
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
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
}