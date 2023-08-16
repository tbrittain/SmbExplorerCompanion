using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerOverview
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string DisplayPosition => $"{PrimaryPosition}{(IsPitcher ? $" ({PitcherRole})" : "")}";
    public string? PitcherRole { get; set; }
    public string Chemistry { get; set; } = string.Empty;
    public string CurrentTeam { get; set; } = string.Empty;
    public int? CurrentTeamId { get; set; }

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
    public ObservableCollection<PlayerAwardBase> Awards { get; set; } = new();

    public void PopulateCareerStats()
    {
        if (IsPitcher)
            PopulateCareerPitchingStats();
        else
            PopulateCareerBattingStats();

        PopulateCareerAwards();
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

    private void PopulateCareerAwards()
    {
        var minSeason = PlayerSeasonBatting.Any() 
            ? PlayerSeasonBatting.Min(x => x.SeasonNumber)
            : PlayerSeasonPitching.Min(x => x.SeasonNumber);
        var maxSeason = PlayerSeasonBatting.Any() 
            ? PlayerSeasonBatting.Max(x => x.SeasonNumber)
            : PlayerSeasonPitching.Max(x => x.SeasonNumber);

        for (var season = minSeason; season <= maxSeason; season++)
        {
            // here, we will aggregate all of the awards for each given season
            // this is necessary because if we were to concat, then we may get duplicates
            // if we concat the awards for a season and a playoff, for example
            // if we union, then we may lost awards that are the same for different seasons
        }
    }

    public ObservableCollection<PlayerCareerBattingOverview> PlayerCareerBatting { get; set; } = new();
    public ObservableCollection<PlayerCareerPitchingOverview> PlayerCareerPitching { get; set; } = new();

    public ObservableCollection<PlayerBattingOverview> PlayerSeasonBatting { get; set; } = new();
    public ObservableCollection<PlayerBattingOverview> PlayerPlayoffBatting { get; set; } = new();
    public ObservableCollection<PlayerPitchingOverview> PlayerSeasonPitching { get; set; } = new();
    public ObservableCollection<PlayerPitchingOverview> PlayerPlayoffPitching { get; set; } = new();
    public ObservableCollection<PlayerGameStatOverview> GameStats { get; set; } = new();
}