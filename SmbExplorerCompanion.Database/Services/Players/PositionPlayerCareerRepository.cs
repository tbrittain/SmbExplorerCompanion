using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Entities;
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

    public async Task<OneOf<List<PlayerCareerBattingDto>, Exception>> GetBattingCareers(
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        bool onlyHallOfFamers = false,
        int? primaryPositionId = null,
        bool onlyActivePlayers = false,
        CancellationToken cancellationToken = default)
    {
        if (playerId is not null && pageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        if (playerId is not null && onlyHallOfFamers)
            throw new ArgumentException("Cannot provide both PlayerId and OnlyHallOfFamers");

        if (playerId is not null && primaryPositionId is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PrimaryPositionId");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var limitValue = limit ?? 30;

        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerBattingDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        try
        {
            if (primaryPositionId is not null)
            {
                var position = await _dbContext.Positions
                    .Where(x => x.IsPrimaryPosition)
                    .SingleOrDefaultAsync(x => x.Id == primaryPositionId, cancellationToken);

                if (position is null)
                    return new ArgumentException($"No primary position found with Id {primaryPositionId}");
            }

            var mostRecentSeason = await _dbContext.Seasons
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.Team)
                .Where(x => x.SeasonTeamHistory.First().Team.FranchiseId == franchiseId)
                .OrderByDescending(x => x.Id)
                .FirstAsync(cancellationToken);

            var queryable = GetCareerBattingIQueryable()
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => playerId == null || x.Id == playerId)
                .Where(x => !onlyHallOfFamers || x.IsHallOfFamer)
                .Where(x => !onlyActivePlayers || x.PlayerSeasons
                    .OrderByDescending(y => y.Id)
                    .First().SeasonId == mostRecentSeason.Id)
                .Where(x => primaryPositionId == null || x.PrimaryPositionId == primaryPositionId);

            var playerBattingDtos = await GetCareerBattingDtos(queryable)
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * limitValue)
                .Take(limitValue)
                .ToListAsync(cancellationToken: cancellationToken);

            // Calculate the rate stats that we omitted above
            foreach (var battingDto in playerBattingDtos)
            {
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
                    foreach (var _ in Enumerable.Range(1, battingDto.NumChampionships))
                    {
                        battingDto.Awards.Add(new PlayerAwardBaseDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }

                if (battingDto.IsHallOfFamer)
                    battingDto.Awards.Add(new PlayerAwardBaseDto
                    {
                        Id = -1,
                        Name = "Hall of Fame",
                        Importance = 0,
                        OmitFromGroupings = false
                    });
            }

            return playerBattingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerCareerBattingDto>, None, Exception>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var season = await _dbContext.Seasons
                .Where(x => x.Id == seasonId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (season is null)
                return new ArgumentException($"Season with id {seasonId} not found.");

            var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

            var allFranchiseSeasons = await _dbContext.Seasons
                .Where(x => x.FranchiseId == franchiseId)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken: cancellationToken);

            // Since the purpose of this method is to return player careers who have played in the past
            // but not in the current season, we can return an empty list if the season provided is the minimum
            // season for the franchise.
            if (season.Id == allFranchiseSeasons.First().Id)
                return new None();

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
                    foreach (var _ in Enumerable.Range(1, battingDto.NumChampionships))
                    {
                        battingDto.Awards.Add(new PlayerAwardBaseDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }
            }

            return battingDtos
                .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
                .ToList();
        }
        catch (Exception e)
        {
            return e;
        }
    }


    public async Task<OneOf<List<SimilarPlayerDto>, Exception>> GetSimilarBattingCareers(int playerId,
        CancellationToken cancellationToken = default)
    {
        var playerCareerBattingResult = await GetBattingCareers(playerId: playerId, cancellationToken: cancellationToken);
        if (playerCareerBattingResult.TryPickT1(out var exception, out var playerCareer))
            return exception;

        if (!playerCareer.Any())
            return new Exception($"No player career found for player ID {playerId}");

        var playerCareerBatting = playerCareer.First();

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
                BatHandedness = x.BatHandedness.Name,
                ThrowHandedness = x.ThrowHandedness.Name,
                PrimaryPosition = x.PrimaryPosition.Name,
                Chemistry = x.Chemistry!.Name,
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
                Name = x.PlayerName,
                SimilarityScore = x.SimilarityScore
            })
            .ToList();
    }

    private IQueryable<Player> GetCareerBattingIQueryable()
    {
        return _dbContext.Players
            .Include(x => x.Chemistry)
            .Include(x => x.BatHandedness)
            .Include(x => x.ThrowHandedness)
            .Include(x => x.PrimaryPosition)
            .Include(x => x.PitcherRole)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Season)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.BattingStats)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Awards)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.ChampionshipWinner);
    }

    private static IQueryable<PlayerCareerBattingDto> GetCareerBattingDtos(IQueryable<Player> players, bool omitRunnerUps = true)
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
                BatHandedness = x.BatHandedness.Name,
                BatHandednessId = x.BatHandednessId,
                ThrowHandedness = x.ThrowHandedness.Name,
                ThrowHandednessId = x.ThrowHandednessId,
                PrimaryPosition = x.PrimaryPosition.Name,
                PrimaryPositionId = x.PrimaryPositionId,
                PitcherRole = x.PitcherRole != null ? x.PitcherRole.Name : null,
                ChemistryId = x.ChemistryId,
                Chemistry = x.Chemistry!.Name,
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
                Awards = x.PlayerSeasons
                    .SelectMany(y => y.Awards)
                    .Where(y => !omitRunnerUps || !y.OmitFromGroupings)
                    .Select(y => new PlayerAwardBaseDto
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Importance = y.Importance,
                        OmitFromGroupings = y.OmitFromGroupings
                    })
                    .ToList(),
                IsHallOfFamer = x.IsHallOfFamer,
                NumChampionships = x.PlayerSeasons
                    .Count(y => y.ChampionshipWinner != null)
            });
    }
}