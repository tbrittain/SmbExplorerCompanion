using System.Linq;
using System.Threading.Tasks;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Players;

// ReSharper disable once CheckNamespace
namespace SmbExplorerCompanion.WPF.Services;

public partial class MappingService
{
    public async Task<PlayerBattingCareer> FromCore(PlayerCareerBattingDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = primaryPositionId != default
            ? await _lookupSearchService.GetPositionById(primaryPositionId)
            : default;
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

        var awards = x.AwardIds
            .Select(async y => await _lookupSearchService.GetPlayerAwardById(y))
            .Select(y => y.Result)
            .ToList();

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
            IsHallOfFamer = x.IsHallOfFamer, NumSeasons = x.NumSeasons,
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
            Awards = awards,
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position?.Name ?? string.Empty, pitcherRole?.Name)
        };
    }

    public async Task<PlayerPitchingCareer> FromCore(PlayerCareerPitchingDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = primaryPositionId != default
            ? await _lookupSearchService.GetPositionById(primaryPositionId)
            : default;
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

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
            IsHallOfFamer = x.IsHallOfFamer, NumSeasons = x.NumSeasons,
            Wins = x.Wins,
            Losses = x.Losses,
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
            Awards = x.AwardIds
                .Select(async y => await _lookupSearchService.GetPlayerAwardById(y))
                .Select(y => y.Result)
                .ToList(),
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position?.Name ?? string.Empty, pitcherRole?.Name)
        };
    }

    public async Task<PlayerCareerBase> FromCore(PlayerCareerBaseDto x)
    {
        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = await _lookupSearchService.GetPositionById(primaryPositionId);
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

        return new PlayerCareerBase
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
            StartSeasonNumber = x.StartSeasonNumber,
            EndSeasonNumber = x.EndSeasonNumber,
            NumSeasons = x.NumSeasons,
            IsRetired = x.IsRetired,
            Age = x.Age,
            RetiredCurrentAge = x.RetiredCurrentAge,
            IsHallOfFamer = x.IsHallOfFamer,
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position.Name, pitcherRole?.Name)
        };
    }

    public async Task<PlayerOverview> FromCore(PlayerOverviewDto x)
    {
        var careerPitching = await FromCore(x.CareerPitching);
        var careerBatting = await FromCore(x.CareerBatting);

        var pitcherRoleId = x.PitcherRoleId;
        var primaryPositionId = x.PrimaryPositionId;

        var position = await _lookupSearchService.GetPositionById(primaryPositionId);
        var pitcherRole = pitcherRoleId.HasValue
            ? await _lookupSearchService.GetPitcherRoleById(pitcherRoleId.Value)
            : null;

        var gameStats = x.GameStats
            .Select(async y => await FromCore(y))
            .Select(y => y.Result)
            .ToObservableCollection();

        var awards = x.AwardIds
            .Select(async y => await _lookupSearchService.GetPlayerAwardById(y))
            .Select(y => y.Result)
            .ToList();

        var playerSeasonBatting = x.PlayerSeasonBatting
            .Select(async y => await FromCore(y))
            .Select(y => y.Result)
            .ToObservableCollection();

        var playerSeasonPitching = x.PlayerSeasonPitching
            .Select(async y => await FromCore(y))
            .Select(y => y.Result)
            .ToObservableCollection();

        var playerPlayoffBatting = x.PlayerPlayoffBatting
            .Select(async y => await FromCore(y))
            .Select(y => y.Result)
            .ToObservableCollection();

        var playerPlayoffPitching = x.PlayerPlayoffPitching
            .Select(async y => await FromCore(y))
            .Select(y => y.Result)
            .ToObservableCollection();

        var overview = new PlayerOverview
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
            StartSeasonNumber = x.StartSeasonNumber,
            EndSeasonNumber = x.EndSeasonNumber,
            IsRetired = x.IsRetired,
            Age = x.Age,
            RetiredCurrentAge = x.RetiredCurrentAge,
            IsHallOfFamer = x.IsHallOfFamer,
            NumSeasons = x.NumSeasons,
            NumChampionships = x.NumChampionships,
            CurrentTeamId = x.CurrentTeamId,
            CurrentTeam = x.CurrentTeam,
            Awards = awards,
            CareerBatting = careerBatting,
            CareerPitching = careerPitching,
            PlayerSeasonBatting = playerSeasonBatting,
            PlayerSeasonPitching = playerSeasonPitching,
            PlayerPlayoffBatting = playerPlayoffBatting,
            PlayerPlayoffPitching = playerPlayoffPitching,
            GameStats = gameStats,
            DisplayPrimaryPosition = PlayerDetailBaseExtensions.GetDisplayPrimaryPosition(position.Name, pitcherRole?.Name)
        };

        overview.PitcherRole = pitcherRole?.Name;
        overview.PrimaryPosition = position.Name;
        overview.BatHandedness = (await _lookupSearchService.GetBatHandednessById(x.BatHandednessId)).Name;
        overview.ThrowHandedness = (await _lookupSearchService.GetThrowHandednessById(x.ThrowHandednessId)).Name;
        if (x.ChemistryId.HasValue)
            overview.Chemistry = (await _lookupSearchService.GetChemistryById(x.ChemistryId.Value)).Name;

        return overview;
    }

    public async Task<PlayerGameStatOverview> FromCore(PlayerGameStatOverviewDto x)
    {
        var traitTasks = x.TraitIds
            .Select(async y => await _lookupSearchService.GetTraitById(y))
            .ToList();
        await Task.WhenAll(traitTasks);

        var traits = traitTasks
            .Select(y => y.Result)
            .ToObservableCollection();

        return new PlayerGameStatOverview
        {
            SeasonId = x.SeasonId,
            SeasonNumber = x.SeasonNumber,
            Age = x.Age,
            TeamNames = x.TeamNames,
            Power = x.Power,
            Contact = x.Contact,
            Speed = x.Speed,
            Fielding = x.Fielding,
            Arm = x.Arm,
            Velocity = x.Velocity,
            Junk = x.Junk,
            Accuracy = x.Accuracy,
            Salary = x.Salary,
            SecondaryPosition = x.SecondaryPosition,
            Traits = string.Join(", ", traits.Select(y => y.Name))
        };
    }
}