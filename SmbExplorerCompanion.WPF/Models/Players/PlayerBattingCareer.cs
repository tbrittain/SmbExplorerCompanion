using System.Collections.Generic;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerBattingCareer : PlayerCareerBase
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
    public List<PlayerAwardBase> Awards { get; set; } = new();
    public string? DisplayAwards => Awards.GetFormattedAwards(isSeason: false);
}

public static class PlayerBattingCareerExtensions
{
    public static PlayerBattingCareer FromCore(this PlayerCareerBattingDto x)
    {
        return new PlayerBattingCareer
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
            AtBats = x.AtBats,
            Hits = x.Hits,
            Singles = x.Singles,
            Doubles = x.Doubles,
            Triples = x.Triples,
            HomeRuns = x.HomeRuns,
            Walks = x.Walks,
            BattingAverage = x.BattingAverage,
            Runs = x.Runs,
            RunsBattedIn = x.RunsBattedIn,
            StolenBases = x.StolenBases,
            HitByPitch = x.HitByPitch,
            SacrificeHits = x.SacrificeHits,
            SacrificeFlies = x.SacrificeFlies,
            Obp = x.Obp,
            Slg = x.Slg,
            Ops = x.Ops,
            OpsPlus = x.OpsPlus,
            Errors = x.Errors,
            Strikeouts = x.Strikeouts,
            Awards = x.Awards
                .Select(y => y.FromCore())
                .ToList()
        };
    }
}