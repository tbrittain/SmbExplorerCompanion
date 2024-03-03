using System.Collections.Generic;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerPitchingCareer : PlayerCareerBase
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int GamesStarted { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public int Walks { get; set; }
    public int Hits { get; set; }
    public int HomeRuns { get; set; }
    public int EarnedRuns { get; set; }
    public int TotalPitches { get; set; }
    public int HitByPitch { get; set; }
    public double EraMinus { get; set; }
    public double FipMinus { get; set; }
    public double Era { get; set; }
    public double Fip { get; set; }
    public double Whip { get; set; }
    public List<PlayerAwardBase> Awards { get; set; } = new();
    public string? DisplayAwards => Awards.GetFormattedAwards(isSeason: false);
}

public static class PlayerPitchingCareerExtensions
{
    public static PlayerPitchingCareer FromCore(this PlayerCareerPitchingDto x, LookupSearchService lss)
    {
        return new PlayerPitchingCareer
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
            Age = x.Age,
            StartSeasonNumber = x.StartSeasonNumber,
            EndSeasonNumber = x.EndSeasonNumber,
            IsRetired = x.IsRetired,
            RetiredCurrentAge = x.RetiredCurrentAge,
            IsHallOfFamer = x.IsHallOfFamer,NumSeasons = x.NumSeasons,
            Wins = x.Wins,
            Losses = x.Losses,
            EarnedRuns = x.EarnedRuns,
            TotalPitches = x.TotalPitches,
            Era = x.Era,
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
            Awards = x.Awards
                .Select(y => y.FromCore())
                .ToList(),
            DisplayPrimaryPosition = x.GetDisplayPrimaryPosition(lss)
        };
    }
}