using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.Database.Entities;
using SmbExplorerCompanion.Shared.Enums;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services.Players;

public class PositionPlayerCareerRepository : IPositionPlayerCareerRepository
{
    private readonly IApplicationContext _applicationContext;
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PositionPlayerCareerRepository(IApplicationContext applicationContext, SmbExplorerCompanionDbContext dbContext)
    {
        _applicationContext = applicationContext;
        _dbContext = dbContext;
    }

    public async Task<List<PlayerCareerBattingDto>> GetBattingCareers(
        GetBattingCareersFilters filters,
        CancellationToken cancellationToken = default)
    {
        if (filters.PlayerId is not null && filters.PageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        if (filters.PlayerId is not null && filters.OnlyHallOfFamers)
            throw new ArgumentException("Cannot provide both PlayerId and OnlyHallOfFamers");

        if (filters.PlayerId is not null && filters.PrimaryPositionId is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PrimaryPositionId");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var limitValue = filters.Limit ?? 30;

        var orderBy = filters.OrderBy;
        if (filters.OrderBy is not null)
        {
            orderBy += filters.Descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerBattingDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        if (filters.PrimaryPositionId is not null)
        {
            var position = await _dbContext.Positions
                .Where(x => x.IsPrimaryPosition)
                .SingleOrDefaultAsync(x => x.Id == filters.PrimaryPositionId, cancellationToken);

            if (position is null)
                throw new ArgumentException($"No primary position found with Id {filters.PrimaryPositionId}");
        }

        var mostRecentSeason = await _dbContext.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .OrderByDescending(x => x.Id)
            .FirstAsync(cancellationToken);

        List<int> activePlayerIds = new();
        if (filters.OnlyActivePlayers)
        {
            activePlayerIds = await _dbContext.Players
                .Include(x => x.PlayerSeasons)
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => x.PlayerSeasons.Any(y => y.SeasonId == mostRecentSeason.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        var playerBattingDtos = await _dbContext.PlayerSeasonBattingStats
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .Where(x => x.PlayerSeason.Player.FranchiseId == franchiseId)
            .Where(x => filters.PlayerId == null || x.PlayerSeason.PlayerId == filters.PlayerId)
            .Where(x => !filters.OnlyHallOfFamers || x.PlayerSeason.Player.IsHallOfFamer)
            .Where(x => !filters.OnlyActivePlayers || activePlayerIds.Contains(x.PlayerSeason.PlayerId))
            .Where(x => filters.PrimaryPositionId == null || x.PlayerSeason.Player.PrimaryPositionId == filters.PrimaryPositionId)
            .Where(x => filters.Seasons == null || (
                    x.PlayerSeason.SeasonId >= filters.Seasons.Value.StartSeasonId &&
                    x.PlayerSeason.SeasonId <= filters.Seasons.Value.EndSeasonId
                )
            )
            .Where(x => filters.ChemistryId == null || x.PlayerSeason.Player.ChemistryId == filters.ChemistryId)
            .Where(x => filters.BatHandednessId == null || x.PlayerSeason.Player.BatHandednessId == filters.BatHandednessId)
            .Where(x => filters.ThrowHandednessId == null || x.PlayerSeason.Player.ThrowHandednessId == filters.ThrowHandednessId)
            .Where(x => filters.IsPlayoffs == false || x.IsRegularSeason == false)
            .GroupBy(x => x.PlayerSeason.PlayerId)
            .Select(x => new PlayerCareerBattingDto
            {
                PlayerId = x.Key,
                StartSeasonNumber = x.Min(y => y.PlayerSeason.Season.Number),
                EndSeasonNumber = x.Max(y => y.PlayerSeason.Season.Number),
                Age = x.Max(y => y.PlayerSeason.Age),
                NumSeasons = x.Count(y => y.IsRegularSeason),
                AtBats = x.Sum(y => y.AtBats),
                Hits = x.Sum(y => y.Hits),
                HomeRuns = x.Sum(y => y.HomeRuns),
                // Calculate rate stats in the application layer, as we will not be sorting by them
                Runs = x.Sum(y => y.Runs),
                RunsBattedIn = x.Sum(y => y.RunsBattedIn),
                StolenBases = x.Sum(y => y.StolenBases),
                WeightedOpsPlusOrEraMinus = x
                    .Sum(y => ((y.OpsPlus ?? 0) - 95) * y.PlateAppearances * BattingScalingFactor +
                              (y.StolenBases - y.CaughtStealing) * BaserunningScalingFactor),
                OpsPlus = x
                    .Sum(y => y.PlateAppearances) == 0
                    ? 0
                    : x.Sum(y => (y.OpsPlus ?? 0) * y.PlateAppearances) /
                      x.Sum(y => y.PlateAppearances),
                Singles = x.Sum(y => y.Singles),
                Doubles = x.Sum(y => y.Doubles),
                Triples = x.Sum(y => y.Triples),
                Walks = x.Sum(y => y.Walks),
                Strikeouts = x.Sum(y => y.Strikeouts),
                GamesPlayed = x.Sum(y => y.GamesPlayed),
                HitByPitch = x.Sum(y => y.HitByPitch),
                SacrificeHits = x.Sum(y => y.SacrificeHits),
                SacrificeFlies = x.Sum(y => y.SacrificeFlies),
                Errors = x.Sum(y => y.Errors),
                AwardIds = x
                    .SelectMany(y => y.PlayerSeason.Awards)
                    .Where(y => !y.OmitFromGroupings)
                    .Select(y => y.Id)
                    .ToList(),
                NumChampionships = x.Count(y => y.PlayerSeason.ChampionshipWinner != null),
                TotalSalary = x
                    .Sum(y => y.IsRegularSeason == false
                        ? 0
                        : y.PlayerSeason.PlayerTeamHistory
                            .Single(z => z.Order == 1).SeasonTeamHistoryId == null
                            ? 0
                            : y.PlayerSeason.Salary),
            })
            .OrderBy(orderBy)
            .Skip(((filters.PageNumber ?? 1) - 1) * limitValue)
            .Take(limitValue)
            .ToListAsync(cancellationToken: cancellationToken);

        var playerIds = playerBattingDtos
            .Select(x => x.PlayerId)
            .Distinct()
            .ToList();
        var players = await _dbContext.Players
            .Where(x => playerIds.Contains(x.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        // Calculate the rate stats that we omitted above
        foreach (var battingDto in playerBattingDtos)
        {
            var player = players.Single(x => x.Id == battingDto.PlayerId);

            battingDto.PlayerName = $"{player.FirstName} {player.LastName}";
            battingDto.IsPitcher = player.PitcherRoleId != null;
            battingDto.BatHandednessId = player.BatHandednessId;
            battingDto.ThrowHandednessId = player.ThrowHandednessId;
            battingDto.PrimaryPositionId = player.PrimaryPositionId;
            battingDto.ChemistryId = player.ChemistryId;
            battingDto.IsHallOfFamer = player.IsHallOfFamer;

            battingDto.IsRetired = battingDto.EndSeasonNumber < mostRecentSeason.Number;
            if (battingDto.IsRetired) battingDto.RetiredCurrentAge = battingDto.Age + (mostRecentSeason.Number - battingDto.EndSeasonNumber);

            battingDto.BattingAverage = battingDto.AtBats == 0
                ? 0
                : battingDto.Hits / (double) battingDto.AtBats;
            battingDto.Obp = battingDto.AtBats == 0
                ? 0
                : (battingDto.Hits + battingDto.Walks + battingDto.HitByPitch) /
                  (double) (battingDto.AtBats + battingDto.Walks + battingDto.HitByPitch +
                            battingDto.SacrificeFlies);
            battingDto.Slg = battingDto.AtBats == 0
                ? 0
                : (battingDto.Singles + battingDto.Doubles * 2 + battingDto.Triples * 3 +
                   battingDto.HomeRuns * 4) / (double) battingDto.AtBats;
            battingDto.Ops = battingDto.Obp + battingDto.Slg;

            if (battingDto.NumChampionships > 0)
            {
                foreach (var _ in Enumerable.Range(1, battingDto.NumChampionships))
                {
                    battingDto.AwardIds.Add((int) VirtualAward.Champion);
                }
            }

            if (battingDto.IsHallOfFamer)
            {
                battingDto.AwardIds.Add((int) VirtualAward.HallOfFame);
            }
        }

        return playerBattingDtos;
    }

    public async Task<List<PlayerCareerBattingDto>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default)
    {
        var season = await _dbContext.Seasons
            .Where(x => x.Id == seasonId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (season is null)
            throw new ArgumentException($"Season with id {seasonId} not found.");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var allFranchiseSeasons = await _dbContext.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        // Since the purpose of this method is to return player careers who have played in the past
        // but not in the current season, we can return an empty list if the season provided is the minimum
        // season for the franchise.
        if (season.Id == allFranchiseSeasons.First().Id)
            return new List<PlayerCareerBattingDto>();

        var previousSeason = allFranchiseSeasons
            .Where(x => x.Id < season.Id)
            .OrderByDescending(x => x.Id)
            .First();

        var mostRecentSeason = allFranchiseSeasons
            .OrderByDescending(x => x.Id)
            .First();

        // Here, we are going to get all of the player IDs that have a player season in the previous season,
        // but lack one in the season queried

        var retiredPlayers = await _dbContext.PlayerSeasons
            .Where(x => x.SeasonId == previousSeason.Id)
            .Select(x => x.PlayerId)
            .Except(_dbContext.PlayerSeasons
                .Where(x => x.SeasonId == season.Id)
                .Select(x => x.PlayerId))
            .ToListAsync(cancellationToken: cancellationToken);

        var battingQueryable = GetCareerBattingIQueryable()
            .Where(x => x.PitcherRoleId == null)
            .Where(x => retiredPlayers.Contains(x.Id));

        var battingDtos = await GetCareerBattingDtos(battingQueryable, false)
            .ToListAsync(cancellationToken: cancellationToken);

        // Calculate the rate stats that we omitted above
        foreach (var battingDto in battingDtos)
        {
            battingDto.IsRetired = true;
            battingDto.RetiredCurrentAge = battingDto.Age + (mostRecentSeason.Number - battingDto.EndSeasonNumber);
            battingDto.BattingAverage = battingDto.AtBats == 0
                ? 0
                : battingDto.Hits / (double) battingDto.AtBats;
            battingDto.Obp = battingDto.AtBats == 0
                ? 0
                : (battingDto.Hits + battingDto.Walks + battingDto.HitByPitch) /
                  (double) (battingDto.AtBats + battingDto.Walks + battingDto.HitByPitch +
                            battingDto.SacrificeFlies);
            battingDto.Slg = battingDto.AtBats == 0
                ? 0
                : (battingDto.Singles + battingDto.Doubles * 2 + battingDto.Triples * 3 +
                   battingDto.HomeRuns * 4) / (double) battingDto.AtBats;
            battingDto.Ops = battingDto.Obp + battingDto.Slg;

            if (battingDto.NumChampionships > 0)
            {
                foreach (var _ in Enumerable.Range(1, battingDto.NumChampionships))
                {
                    battingDto.AwardIds.Add((int) VirtualAward.Champion);
                }
            }
        }

        return battingDtos
            .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
            .ToList();
    }


    public async Task<List<SimilarPlayerDto>> GetSimilarBattingCareers(int playerId,
        CancellationToken cancellationToken = default)
    {
        var battingCareers = await GetBattingCareers(
            new GetBattingCareersFilters
            {
                PlayerId = playerId
            },
            cancellationToken: cancellationToken);

        if (!battingCareers.Any())
            throw new Exception($"No player career found for player ID {playerId}");

        var playerCareerBatting = battingCareers.First();

        var gamesPlayed = playerCareerBatting.GamesPlayed;
        var atBats = playerCareerBatting.AtBats;
        var runs = playerCareerBatting.Runs;
        var hits = playerCareerBatting.Hits;
        var doubles = playerCareerBatting.Doubles;
        var triples = playerCareerBatting.Triples;
        var homeRuns = playerCareerBatting.HomeRuns;
        var runsBattedIn = playerCareerBatting.RunsBattedIn;
        var walks = playerCareerBatting.Walks;
        var strikeouts = playerCareerBatting.Strikeouts;
        var stolenBases = playerCareerBatting.StolenBases;
        var battingAverage = playerCareerBatting.BattingAverage;
        var slg = playerCareerBatting.Slg;

        var chemistryId = playerCareerBatting.ChemistryId;
        var throwHandednessId = playerCareerBatting.ThrowHandednessId;
        var batHandednessId = playerCareerBatting.BatHandednessId;
        var primaryPositionId = playerCareerBatting.PrimaryPositionId;

        var queryable = GetCareerBattingIQueryable()
            .Where(x => x.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
            .Where(x => x.Id != playerId);

        var similarBatters = await queryable
            .Select(x => new
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                AtBats = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.AtBats)),
                Hits = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Hits)),
                HomeRuns = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HomeRuns)),
                Runs = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Runs)),
                RunsBattedIn = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.RunsBattedIn)),
                StolenBases = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.StolenBases)),
                Singles = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Singles)),
                Doubles = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Doubles)),
                Triples = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Triples)),
                Walks = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Walks)),
                Strikeouts = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Strikeouts)),
                GamesPlayed = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.GamesPlayed)),
                BattingAverage = x.PlayerSeasons.Average(y => y.BattingStats.Average(z => z.BattingAverage ?? 0
                )),
                Slg = x.PlayerSeasons.Average(y => y.BattingStats.Average(z => z.Slg ?? 0)),
                SimilarityScore =
                    1000 - (
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.GamesPlayed)) - gamesPlayed) / 20D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.AtBats)) - atBats) / 75D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Runs)) - runs) / 10D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Hits)) - hits) / 15D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Doubles)) - doubles) / 5D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Triples)) - triples) / 4D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HomeRuns)) - homeRuns) / 2D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.RunsBattedIn)) - runsBattedIn) / 10D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Walks)) - walks) / 25D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Strikeouts)) - strikeouts) / 150D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.StolenBases)) - stolenBases) / 20D +
                        Math.Abs(x.PlayerSeasons.Average(y => y.BattingStats.Average(z => z.BattingAverage ?? 0)) - battingAverage) * 100 +
                        Math.Abs(x.PlayerSeasons.Average(y => y.BattingStats.Average(z => z.Slg ?? 0)) - slg) / 2 * 100
                    ) -
                    (x.PrimaryPositionId != primaryPositionId ? 50 : 0) -
                    (x.ChemistryId != chemistryId ? 50 : 0) -
                    (x.BatHandednessId != batHandednessId ? 30 : 0) -
                    (x.ThrowHandednessId != throwHandednessId ? 30 : 0)
            })
            .OrderByDescending(x => x.SimilarityScore)
            .Take(10)
            .ToListAsync(cancellationToken: cancellationToken);

        return similarBatters
            .Select(x => new SimilarPlayerDto
            {
                PlayerId = x.PlayerId,
                PlayerName = x.PlayerName,
                SimilarityScore = x.SimilarityScore
            })
            .ToList();
    }

    private IQueryable<Player> GetCareerBattingIQueryable()
    {
        return _dbContext.Players
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.BattingStats);
    }

    private static IQueryable<PlayerCareerBattingDto> GetCareerBattingDtos(
        IQueryable<Player> players,
        bool omitRunnerUpAwards = true)
    {
        return players
            .Select(x => new PlayerCareerBattingDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                IsPitcher = x.PitcherRole != null,
                TotalSalary = x.PlayerSeasons
                    .Sum(y => y.PlayerTeamHistory
                        .Single(z => z.Order == 1).SeasonTeamHistoryId == null
                        ? 0
                        : y.Salary),
                BatHandednessId = x.BatHandednessId,
                ThrowHandednessId = x.ThrowHandednessId,
                PrimaryPositionId = x.PrimaryPositionId,
                ChemistryId = x.ChemistryId,
                StartSeasonNumber = x.PlayerSeasons.Min(y => y.Season.Number),
                EndSeasonNumber = x.PlayerSeasons.Max(y => y.Season.Number),
                Age = x.PlayerSeasons.Max(y => y.Age),
                NumSeasons = x.PlayerSeasons.Count,
                AtBats = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.AtBats)),
                Hits = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Hits)),
                HomeRuns = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HomeRuns)),
                // Calculate rate stats in the application layer, as we will not be sorting by them
                Runs = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Runs)),
                RunsBattedIn = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.RunsBattedIn)),
                StolenBases = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.StolenBases)),
                WeightedOpsPlusOrEraMinus = x.PlayerSeasons
                    .SelectMany(y => y.BattingStats)
                    .Sum(y => ((y.OpsPlus ?? 0) - 95) * y.PlateAppearances * BattingScalingFactor +
                              (y.StolenBases - y.CaughtStealing) * BaserunningScalingFactor),
                OpsPlus = x.PlayerSeasons
                    .SelectMany(y => y.BattingStats)
                    .Sum(y => y.PlateAppearances) == 0
                    ? 0
                    : x.PlayerSeasons
                          .SelectMany(y => y.BattingStats)
                          .Sum(y => (y.OpsPlus ?? 0) * y.PlateAppearances)
                      /
                      x.PlayerSeasons
                          .SelectMany(y => y.BattingStats)
                          .Sum(y => y.PlateAppearances),
                Singles = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Singles)),
                Doubles = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Doubles)),
                Triples = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Triples)),
                Walks = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Walks)),
                Strikeouts = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Strikeouts)),
                GamesPlayed = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.GamesPlayed)),
                HitByPitch = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HitByPitch)),
                SacrificeHits = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.SacrificeHits)),
                SacrificeFlies = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.SacrificeFlies)),
                Errors = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Errors)),
                AwardIds = x.PlayerSeasons
                    .SelectMany(y => y.Awards)
                    .Where(y => !omitRunnerUpAwards || !y.OmitFromGroupings)
                    .Select(y => y.Id)
                    .ToList(),
                IsHallOfFamer = x.IsHallOfFamer,
                NumChampionships = x.PlayerSeasons
                    .Count(y => y.ChampionshipWinner != null)
            });
    }
}