﻿using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerSeasonBatting : PlayerSeasonBase
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
    public ObservableCollection<PlayerAwardBase> Awards { get; set; } = new();
    public string? DisplayAwards => Awards.GetFormattedAwards(isSeason: true);
}

public static class PlayerSeasonBattingExtensions
{
    public static PlayerSeasonBatting FromCore(this PlayerBattingSeasonDto x, LookupSearchService lss)
    {
        return new PlayerSeasonBatting
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
                .ToObservableCollection(),
            DisplayPrimaryPosition = x.GetDisplayPrimaryPosition(lss)
        };
    }
}