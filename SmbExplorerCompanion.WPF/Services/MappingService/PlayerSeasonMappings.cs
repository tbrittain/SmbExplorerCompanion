﻿using System.Linq;
using System.Threading.Tasks;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;

// ReSharper disable once CheckNamespace
namespace SmbExplorerCompanion.WPF.Services;

public partial class MappingService
{
    public async Task<PlayerSeasonBatting> FromCore(PlayerBattingSeasonDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = await _lookupSearchService.GetPositionById(primaryPositionId);
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

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
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position.Name, pitcherRole?.Name)
        };
    }

    public async Task<PlayerSeasonPitching> FromCore(PlayerPitchingSeasonDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = await _lookupSearchService.GetPositionById(primaryPositionId);
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

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
                .ToObservableCollection(),
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position.Name, pitcherRole?.Name)
        };
    }
}