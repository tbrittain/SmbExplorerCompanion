using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerBattingSeasonDto : PlayerSeasonDto
{
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
    public int Strikeouts { get; set; }
    public List<PlayerAwardBaseDto> Awards { get; set; } = new();
    public bool IsChampion { get; set; }
    public int GamesBatting { get; set; }
    public int PlateAppearances { get; set; }
    public int CaughtStealing { get; set; }
    public int TotalBases { get; set; }
    public string? SecondaryPosition { get; set; } = string.Empty;
    public string Traits { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
}