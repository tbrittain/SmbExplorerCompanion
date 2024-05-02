using System.Linq;
using System.Threading.Tasks;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Players;

// ReSharper disable once CheckNamespace
namespace SmbExplorerCompanion.WPF.Services;

public partial class MappingService
{
    public async Task<PlayerSeasonBatting> FromCore(PlayerBattingSeasonDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = primaryPositionId != default
            ? await _lookupSearchService.GetPositionById(primaryPositionId)
            : default;
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
            Awards = x.AwardIds
                .Select(async y => await _lookupSearchService.GetPlayerAwardById(y))
                .Select(y => y.Result)
                .ToObservableCollection(),
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position?.Name ?? string.Empty, pitcherRole?.Name)
        };
    }

    public async Task<PlayerSeasonPitching> FromCore(PlayerPitchingSeasonDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = primaryPositionId != default
            ? await _lookupSearchService.GetPositionById(primaryPositionId)
            : default;
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

        var pitchTypes = x.PitchTypeIds
            .Select(async y => await _lookupSearchService.GetPitchTypeById(y))
            .Select(y => y.Result)
            .OrderBy(y => y.Name)
            .Select(y => y.Name);

        var awards = x.AwardIds
            .Select(async y => await _lookupSearchService.GetPlayerAwardById(y))
            .Select(y => y.Result)
            .ToObservableCollection();

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
            PitchTypes = string.Join(", ", pitchTypes),
            Awards = awards,
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position?.Name ?? string.Empty, pitcherRole?.Name)
        };
    }
}