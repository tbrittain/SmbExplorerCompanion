namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerOverview
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }

    // Batting stats, applicable to both pitchers and batters
    public int AtBats { get; set; }
    public int Hits { get; set; }
    public int HomeRuns { get; set; }
    public double BattingAverage { get; set; }
    public int Runs { get; set; }
    public int RunsBattedIn { get; set; }
    public int StolenBases { get; set; }
    public double Obp { get; set; }
    public double Slg { get; set; }
    public double Ops { get; set; }
    public double OpsPlus { get; set; }

    // Pitching stats, may apply to position players who have pitched
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double Era { get; set; }
    public int Games { get; set; }
    public int GamesStarted { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public double Whip { get; set; }
    public double EraMinus { get; set; }

    public List<PlayerBattingOverview> PlayerSeasonBatting { get; set; } = new();
    public List<PlayerBattingOverview> PlayerPlayoffBatting { get; set; } = new();
    public List<PlayerPitchingOverview> PlayerSeasonPitching { get; set; } = new();
    public List<PlayerPitchingOverview> PlayerPlayoffPitching { get; set; } = new();
    public List<PlayerSeasonGameStatOverview> GameStats { get; set; } = new();
}