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

public class PitcherCareerRepository : IPitcherCareerRepository
{
    private readonly IApplicationContext _applicationContext;
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PitcherCareerRepository(IApplicationContext applicationContext, SmbExplorerCompanionDbContext dbContext)
    {
        _applicationContext = applicationContext;
        _dbContext = dbContext;
    }

    public async Task<OneOf<List<PlayerCareerPitchingDto>, Exception>> GetPitchingCareers(
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        bool onlyHallOfFamers = false,
        int? pitcherRoleId = null,
        bool onlyActivePlayers = false,
        CancellationToken cancellationToken = default)
    {
        if (playerId is not null && pageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        if (playerId is not null && onlyHallOfFamers)
            throw new ArgumentException("Cannot provide both PlayerId and OnlyHallOfFamers");

        if (playerId is not null && pitcherRoleId is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PrimaryPositionId");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var limitValue = limit ?? 30;

        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerPitchingDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        try
        {
            if (pitcherRoleId is not null)
            {
                var pitcherRole = await _dbContext.PitcherRoles
                    .SingleOrDefaultAsync(x => x.Id == pitcherRoleId, cancellationToken);

                if (pitcherRole is null)
                    return new ArgumentException($"No pitcher role found with Id {pitcherRoleId}");
            }

            var mostRecentSeason = await _dbContext.Seasons
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.Team)
                .Where(x => x.SeasonTeamHistory.First().Team.FranchiseId == franchiseId)
                .OrderByDescending(x => x.Id)
                .FirstAsync(cancellationToken);

            var queryable = GetCareerPitchingIQueryable()
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => x.PitcherRole != null)
                .Where(x => playerId == null || x.Id == playerId)
                .Where(x => !onlyHallOfFamers || x.IsHallOfFamer)
                .Where(x => !onlyActivePlayers || x.PlayerSeasons
                    .OrderByDescending(y => y.Id)
                    .First().SeasonId == mostRecentSeason.Id)
                .Where(x => pitcherRoleId == null || x.PitcherRoleId == pitcherRoleId);

            var playerPitchingDtos = await GetCareerPitchingDtos(queryable)
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * limitValue)
                .Take(limitValue)
                .ToListAsync(cancellationToken: cancellationToken);

            foreach (var pitchingDto in playerPitchingDtos)
            {
                pitchingDto.IsRetired = pitchingDto.EndSeasonNumber < mostRecentSeason.Number;
                {
                    pitchingDto.RetiredCurrentAge = pitchingDto.Age + (mostRecentSeason.Number - pitchingDto.EndSeasonNumber);
                }

                pitchingDto.Era = pitchingDto.InningsPitched == 0
                    ? 0
                    : pitchingDto.EarnedRuns / pitchingDto.InningsPitched * 9;
                pitchingDto.Whip = pitchingDto.InningsPitched == 0
                    ? 0
                    : (pitchingDto.Walks + pitchingDto.Hits) / pitchingDto.InningsPitched;
                pitchingDto.Fip = pitchingDto.InningsPitched == 0
                    ? 0
                    : (13 * pitchingDto.HomeRuns + 3 * (pitchingDto.Walks + pitchingDto.HitByPitch) -
                       2 * pitchingDto.Strikeouts) / pitchingDto.InningsPitched + 3.10;

                if (pitchingDto.NumChampionships > 0)
                    foreach (var _ in Enumerable.Range(1, pitchingDto.NumChampionships))
                    {
                        pitchingDto.Awards.Add(new PlayerAwardBaseDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }

                if (pitchingDto.IsHallOfFamer)
                    pitchingDto.Awards.Add(new PlayerAwardBaseDto
                    {
                        Id = -1,
                        Name = "Hall of Fame",
                        Importance = 0,
                        OmitFromGroupings = false
                    });
            }

            return playerPitchingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerCareerPitchingDto>, None, Exception>> GetHallOfFameCandidates(int seasonId,
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

            var pitchingQueryable = GetCareerPitchingIQueryable()
                .Where(x => x.PitcherRoleId != null)
                .Where(x => retiredPlayers.Contains(x.Id));

            var pitchingDtos = await GetCareerPitchingDtos(pitchingQueryable, false)
                .ToListAsync(cancellationToken: cancellationToken);

            foreach (var pitchingDto in pitchingDtos)
            {
                pitchingDto.IsRetired = true;
                pitchingDto.RetiredCurrentAge = pitchingDto.Age + (mostRecentSeason.Number - pitchingDto.EndSeasonNumber);
                pitchingDto.Era = pitchingDto.InningsPitched == 0
                    ? 0
                    : pitchingDto.EarnedRuns / pitchingDto.InningsPitched * 9;
                pitchingDto.Whip = pitchingDto.InningsPitched == 0
                    ? 0
                    : (pitchingDto.Walks + pitchingDto.Hits) / pitchingDto.InningsPitched;
                pitchingDto.Fip = pitchingDto.InningsPitched == 0
                    ? 0
                    : (13 * pitchingDto.HomeRuns + 3 * (pitchingDto.Walks + pitchingDto.HitByPitch) -
                       2 * pitchingDto.Strikeouts) / pitchingDto.InningsPitched + 3.10;

                if (pitchingDto.NumChampionships > 0)
                    foreach (var _ in Enumerable.Range(1, pitchingDto.NumChampionships))
                    {
                        pitchingDto.Awards.Add(new PlayerAwardBaseDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }
            }

            return pitchingDtos
                .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
                .ToList();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<SimilarPlayerDto>, Exception>> GetSimilarPitchingCareers(int playerId,
        CancellationToken cancellationToken = default)
    {
        var playerCareerPitchingResult = await GetPitchingCareers(playerId: playerId, cancellationToken: cancellationToken);
        if (playerCareerPitchingResult.TryPickT1(out var exception, out var playerCareer))
            return exception;

        if (!playerCareer.Any())
            return new Exception($"No player career found for player ID {playerId}");

        var playerCareerPitching = playerCareer.First();

        var wins = playerCareerPitching.Wins;
        var losses = playerCareerPitching.Losses;
        var era = playerCareerPitching.Era;
        var starts = playerCareerPitching.GamesStarted;
        var completeGames = playerCareerPitching.CompleteGames;
        var inningsPitched = playerCareerPitching.InningsPitched;
        var hits = playerCareerPitching.Hits;
        var strikeouts = playerCareerPitching.Strikeouts;
        var walks = playerCareerPitching.Walks;
        var shutouts = playerCareerPitching.Shutouts;
        var saves = playerCareerPitching.Saves;

        var chemistryId = playerCareerPitching.ChemistryId;
        var throwHandednessId = playerCareerPitching.ThrowHandednessId;
        var pitcherRoleId = playerCareerPitching.PitcherRoleId;

        var queryable = GetCareerPitchingIQueryable()
            .Where(x => x.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
            .Where(x => x.Id != playerId);

        var similarPitchers = await queryable
            .Select(x => new
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                Wins = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Wins)),
                Losses = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Losses)),
                GamesStarted = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.GamesStarted)),
                Saves = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Saves)),
                InningsPitched = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.InningsPitched ?? 0)),
                Strikeouts = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Strikeouts)),
                CompleteGames = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.CompleteGames)),
                Shutouts = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Shutouts)),
                Walks = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Walks)),
                Hits = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Hits)),
                SimilarityScore =
                    1000 - (
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Wins)) - wins) +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Losses)) - losses) / 2D +
                        Math.Max(
                            Math.Abs(x.PlayerSeasons.Average(y => y.PitchingStats.Average(z => z.EarnedRunAverage ?? 0)) - era / 0.02),
                            100
                        ) +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.GamesStarted)) - starts) / 10D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.CompleteGames)) - completeGames) / 20D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.InningsPitched ?? 0)) - inningsPitched) / 50D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Hits)) - hits) / 50D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Strikeouts)) - strikeouts) / 30D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Walks)) - walks) / 10D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Shutouts)) - shutouts) / 5D +
                        Math.Abs(x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Saves)) - saves) / 3D
                    ) -
                    (x.PitcherRoleId != pitcherRoleId ? 50 : 0) -
                    (x.ChemistryId != chemistryId ? 50 : 0) -
                    (x.ThrowHandednessId != throwHandednessId ? 30 : 0)
            })
            .OrderByDescending(x => x.SimilarityScore)
            .Take(10)
            .ToListAsync(cancellationToken: cancellationToken);

        return similarPitchers
            .Select(x => new SimilarPlayerDto
            {
                PlayerId = x.PlayerId,
                PlayerName = x.PlayerName,
                SimilarityScore = x.SimilarityScore
            })
            .ToList();
    }

    private IQueryable<Player> GetCareerPitchingIQueryable()
    {
        return _dbContext.Players
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Season)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.PitchingStats)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Awards)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.ChampionshipWinner);
    }

    private static IQueryable<PlayerCareerPitchingDto> GetCareerPitchingDtos(IQueryable<Player> players, bool omitRunnerUps = true)
    {
        return players
            .Select(x => new PlayerCareerPitchingDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                IsPitcher = x.PitcherRole != null,
                TotalSalary = x.PlayerSeasons
                    .Sum(y => y.PlayerTeamHistory
                        .Single(z => z.Order == 1).SeasonTeamHistoryId == null
                        ? 0
                        : y.Salary),
                PitcherRoleId = x.PitcherRoleId,
                BatHandednessId = x.BatHandednessId,
                ThrowHandednessId = x.ThrowHandednessId,
                PrimaryPositionId = x.PrimaryPositionId,
                ChemistryId = x.ChemistryId,
                StartSeasonNumber = x.PlayerSeasons.Min(y => y.Season.Number),
                EndSeasonNumber = x.PlayerSeasons.Max(y => y.Season.Number),
                Age = x.PlayerSeasons.Max(y => y.Age),
                NumSeasons = x.PlayerSeasons.Count,
                Wins = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Wins)),
                Losses = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Losses)),
                GamesStarted = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.GamesStarted)),
                Saves = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Saves)),
                InningsPitched = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.InningsPitched ?? 0)),
                Strikeouts = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Strikeouts)),
                CompleteGames = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.CompleteGames)),
                Shutouts = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Shutouts)),
                Walks = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Walks)),
                Hits = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Hits)),
                HomeRuns = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.HomeRuns)),
                EarnedRuns = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.EarnedRuns)),
                TotalPitches = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.TotalPitches)),
                HitByPitch = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.HitByPitch)),
                WeightedOpsPlusOrEraMinus = x.PlayerSeasons
                    .SelectMany(y => y.PitchingStats)
                    .Sum(y => (((y.EraMinus ?? 0) + (y.FipMinus ?? 0)) / 2 - 95) * (y.InningsPitched ?? 0) * PitchingScalingFactor),
                EraMinus =
                    x.PlayerSeasons
                        .SelectMany(y => y.PitchingStats)
                        .Sum(y => (y.InningsPitched ?? 0)) > 0
                        ? (x.PlayerSeasons
                               .SelectMany(y => y.PitchingStats)
                               .Sum(y => (y.EraMinus ?? 0) * (y.InningsPitched ?? 0))
                           /
                           x.PlayerSeasons
                               .SelectMany(y => y.PitchingStats)
                               .Sum(y => (y.InningsPitched ?? 0)))
                        : 0,
                FipMinus =
                    x.PlayerSeasons
                        .SelectMany(y => y.PitchingStats)
                        .Sum(y => (y.InningsPitched ?? 0)) > 0
                        ? x.PlayerSeasons
                              .SelectMany(y => y.PitchingStats)
                              .Sum(y => (y.FipMinus ?? 0) * (y.InningsPitched ?? 0))
                          /
                          x.PlayerSeasons
                              .SelectMany(y => y.PitchingStats)
                              .Sum(y => (y.InningsPitched ?? 0))
                        : 0,
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