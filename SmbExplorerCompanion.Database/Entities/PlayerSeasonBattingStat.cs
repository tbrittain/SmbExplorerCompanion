namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSeasonBattingStat
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;

    public int GamesPlayed { get; set; }
    public int GamesBatting { get; set; }
    public int AtBats { get; set; }
    public int PlateAppearances { get; set; }
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int Singles { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int HomeRuns { get; set; }
    public int RunsBattedIn { get; set; }
    public int ExtraBaseHits { get; set; }
    public int TotalBases { get; set; }
    public int StolenBases { get; set; }
    public int CaughtStealing { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public int HitByPitch { get; set; }
    public double? Obp { get; set; }
    public double? Slg { get; set; }
    public double? Ops { get; set; }
    public double? Woba { get; set; }
    public double? Iso { get; set; }
    public double? Babip { get; set; }
    public int SacrificeHits { get; set; }
    public int SacrificeFlies { get; set; }
    public double? BattingAverage { get; set; }
    public int Errors { get; set; }
    public int PassedBalls { get; set; }
    public double? PaPerGame { get; set; }
    public double? AbPerHomeRun { get; set; }
    public double? StrikeoutPercentage { get; set; }
    public double? WalkPercentage { get; set; }
    public double? ExtraBaseHitPercentage { get; set; }
    public double? OpsPlus { get; set; }
    public bool IsRegularSeason { get; set; } = true;
}