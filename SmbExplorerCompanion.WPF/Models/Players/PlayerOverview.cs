using System.Collections.ObjectModel;

namespace SmbExplorerCompanion.WPF.Models.Players;

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

    public void PopulateCareerStats()
    {
        if (IsPitcher)
            PopulateCareerPitchingStats();
        else
            PopulateCareerBattingStats();
    }

    private void PopulateCareerBattingStats()
    {
        PlayerCareerBatting.Add(new PlayerCareerBattingOverview
        {
            AtBats = AtBats,
            Hits = Hits,
            HomeRuns = HomeRuns,
            BattingAverage = BattingAverage,
            Runs = Runs,
            RunsBattedIn = RunsBattedIn,
            StolenBases = StolenBases,
            Obp = Obp,
            Slg = Slg,
            Ops = Ops,
            OpsPlus = OpsPlus
        });
    }

    private void PopulateCareerPitchingStats()
    {
        PlayerCareerPitching.Add(new PlayerCareerPitchingOverview
        {
            Wins = Wins,
            Losses = Losses,
            Era = Era,
            Games = Games,
            GamesStarted = GamesStarted,
            Saves = Saves,
            InningsPitched = InningsPitched,
            Strikeouts = Strikeouts,
            Whip = Whip,
            EraMinus = EraMinus
        });
    }

    public ObservableCollection<PlayerCareerBattingOverview> PlayerCareerBatting { get; set; } = new();
    public ObservableCollection<PlayerCareerPitchingOverview> PlayerCareerPitching { get; set; } = new();

    public ObservableCollection<PlayerBattingOverview> PlayerSeasonBatting { get; set; } = new();
    public ObservableCollection<PlayerBattingOverview> PlayerPlayoffBatting { get; set; } = new();
    public ObservableCollection<PlayerPitchingOverview> PlayerSeasonPitching { get; set; } = new();
    public ObservableCollection<PlayerPitchingOverview> PlayerPlayoffPitching { get; set; } = new();
    public ObservableCollection<PlayerGameStatOverview> GameStats { get; set; } = new();
}