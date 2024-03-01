using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerSeasonPitching : PlayerSeasonBase
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinPercentage { get; set; }
    public int EarnedRuns { get; set; }
    public int TotalPitches { get; set; }
    public double EarnedRunAverage { get; set; }
    public double Fip { get; set; }
    public int GamesStarted { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public int Walks { get; set; }
    public int Hits { get; set; }
    public int HomeRuns { get; set; }
    public int HitByPitch { get; set; }
    public double Whip { get; set; }
    public double EraMinus { get; set; }
    public double FipMinus { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public double HitsPerNine { get; set; }
    public double HomeRunsPerNine { get; set; }
    public double StrikeoutsPerNine { get; set; }
    public double StrikeoutToWalkRatio { get; set; }
    public ObservableCollection<PlayerAwardBase> Awards { get; set; } = new();
    public string? DisplayAwards => Awards.GetFormattedAwards(isSeason: true);
}

public static class PlayerSeasonPitchingExtensions
{
    public static PlayerSeasonPitching FromCore(this PlayerPitchingSeasonDto x)
    {
        return new PlayerSeasonPitching
        {
            PlayerId = x.PlayerId,
            PlayerName = x.PlayerName,
            IsPitcher = x.IsPitcher,
            TotalSalary = x.TotalSalary,
            BatHandednessId = x.BatHandednessId,
            ThrowHandednessId = x.ThrowHandednessId,
            PrimaryPositionId = x.PrimaryPositionId,
            PitcherRoleId = x.PitcherRoleId,
            ChemistryId = x.ChemistryId,
            WeightedOpsPlusOrEraMinus = x.WeightedOpsPlusOrEraMinus,
            SeasonId = x.SeasonId,
            SeasonNumber = x.SeasonNumber,
            TeamNames = x.TeamNames,
            Age = x.Age,
            Wins = x.Wins,
            Losses = x.Losses,
            WinPercentage = x.WinPercentage,
            EarnedRuns = x.EarnedRuns,
            TotalPitches = x.TotalPitches,
            EarnedRunAverage = x.EarnedRunAverage,
            Fip = x.Fip,
            GamesStarted = x.GamesStarted,
            Saves = x.Saves,
            InningsPitched = x.InningsPitched,
            Strikeouts = x.Strikeouts,
            Walks = x.Walks,
            Hits = x.Hits,
            HomeRuns = x.HomeRuns,
            HitByPitch = x.HitByPitch,
            Whip = x.Whip,
            EraMinus = x.EraMinus,
            FipMinus = x.FipMinus,
            CompleteGames = x.CompleteGames,
            Shutouts = x.Shutouts,
            HitsPerNine = x.HitsPerNine,
            HomeRunsPerNine = x.HomeRunsPerNine,
            StrikeoutsPerNine = x.StrikeoutsPerNine,
            StrikeoutToWalkRatio = x.StrikeoutToWalkRatio,
            Awards = x.Awards
                .Select(y => y.FromCore())
                .ToObservableCollection()
        };
    }
}