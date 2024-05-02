using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Entities;
using SmbExplorerCompanion.Shared.Enums;
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

    public async Task<List<PlayerCareerPitchingDto>> GetPitchingCareers(
        GetPitchingCareersFilters filters,
        CancellationToken cancellationToken = default)
    {
        if (filters.PlayerId is not null && filters.PageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        if (filters.PlayerId is not null && filters.OnlyHallOfFamers)
            throw new ArgumentException("Cannot provide both PlayerId and OnlyHallOfFamers");

        if (filters.PlayerId is not null && filters.PitcherRoleId is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PrimaryPositionId");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var limitValue = filters.Limit ?? 30;

        var orderBy = filters.OrderBy;
        if (orderBy is not null)
        {
            orderBy += filters.Descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerPitchingDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        var mostRecentSeason = await _dbContext.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .OrderByDescending(x => x.Id)
            .FirstAsync(cancellationToken);

        var startSeason = await _dbContext.Seasons
            .Where(x => x.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
            .OrderBy(x => x.Id)
            .FirstAsync(cancellationToken: cancellationToken);
        var gamesPerSeason = startSeason.NumGamesRegularSeason;

        List<int> activePlayerIds = new();
        if (filters.OnlyActivePlayers)
            activePlayerIds = await _dbContext.Players
                .Include(x => x.PlayerSeasons)
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => x.PlayerSeasons.Any(y => y.SeasonId == mostRecentSeason.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken: cancellationToken);

        var playerPitchingDtos = await _dbContext.PlayerSeasonPitchingStats
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .Where(x => x.PlayerSeason.Player.FranchiseId == franchiseId)
            .Where(x => x.PlayerSeason.Player.PitcherRoleId != null)
            .Where(x => filters.PlayerId == null || x.PlayerSeason.PlayerId == filters.PlayerId)
            .Where(x => !filters.OnlyHallOfFamers || x.PlayerSeason.Player.IsHallOfFamer)
            .Where(x => !filters.OnlyActivePlayers || activePlayerIds.Contains(x.PlayerSeason.PlayerId))
            .Where(x => filters.Seasons == null || (
                    x.PlayerSeason.SeasonId >= filters.Seasons.Value.StartSeasonId &&
                    x.PlayerSeason.SeasonId <= filters.Seasons.Value.EndSeasonId
                )
            )
            .Where(x => filters.PitcherRoleId == null || x.PlayerSeason.Player.PitcherRoleId == filters.PitcherRoleId)
            .Where(x => filters.ChemistryId == null || x.PlayerSeason.Player.ChemistryId == filters.ChemistryId)
            .Where(x => filters.BatHandednessId == null || x.PlayerSeason.Player.BatHandednessId == filters.BatHandednessId)
            .Where(x => filters.ThrowHandednessId == null || x.PlayerSeason.Player.ThrowHandednessId == filters.ThrowHandednessId)
            .Where(x => filters.IsPlayoffs == false || x.IsRegularSeason == false)
            .GroupBy(x => x.PlayerSeason.PlayerId)
            .Select(x => new PlayerCareerPitchingDto
            {
                PlayerId = x.Key,
                StartSeasonNumber = x.Min(y => y.PlayerSeason.Season.Number),
                EndSeasonNumber = x.Max(y => y.PlayerSeason.Season.Number),
                Age = x.Max(y => y.PlayerSeason.Age),
                NumSeasons = x.Count(y => y.IsRegularSeason),
                Wins = x.Sum(y => y.Wins),
                Losses = x.Sum(y => y.Losses),
                GamesStarted = x.Sum(y => y.GamesStarted),
                Saves = x.Sum(y => y.Saves),
                InningsPitched = x.Sum(y => y.InningsPitched ?? 0),
                Strikeouts = x.Sum(y => y.Strikeouts),
                CompleteGames = x.Sum(y => y.CompleteGames),
                Shutouts = x.Sum(y => y.Shutouts),
                Walks = x.Sum(y => y.Walks),
                Hits = x.Sum(y => y.Hits),
                HomeRuns = x.Sum(y => y.HomeRuns),
                EarnedRuns = x.Sum(y => y.EarnedRuns),
                TotalPitches = x.Sum(y => y.TotalPitches),
                HitByPitch = x.Sum(y => y.HitByPitch),
                WeightedOpsPlusOrEraMinus = x
                    .Sum(y => (((y.EraMinus ?? 0) + (y.FipMinus ?? 0)) / 2 - 95) *
                              (y.InningsPitched ?? 0) * PitchingScalingFactor),
                EraMinus = x
                    .Sum(y => y.InningsPitched ?? 0) > 0
                    ? x
                          .Sum(y => (y.EraMinus ?? 0) * (y.InningsPitched ?? 0))
                      / x
                          .Sum(y => y.InningsPitched ?? 0)
                    : 0,
                FipMinus = x
                    .Sum(y => y.InningsPitched ?? 0) > 0
                    ? x
                          .Sum(y => (y.FipMinus ?? 0) * (y.InningsPitched ?? 0))
                      / x
                          .Sum(y => y.InningsPitched ?? 0)
                    : 0,
                AwardIds = x
                    .Where(y => y.IsRegularSeason)
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
                EarnedRunAverage = x.Sum(y => y.InningsPitched ?? 0) == 0
                    ? 0
                    : x.Sum(y => y.EarnedRuns) /
                    x.Sum(y => y.InningsPitched ?? 0) * 9,
                Whip = x.Sum(y => y.InningsPitched ?? 0) == 0
                    ? 0
                    : (x.Sum(y => y.Walks) + x.Sum(y => y.Hits)) /
                      x.Sum(y => y.InningsPitched ?? 0),
                Fip = x.Sum(y => y.InningsPitched ?? 0) == 0
                    ? 0
                    : (
                        (
                            (
                                (13 * x.Sum(y => y.HomeRuns)) +
                                (3 * (
                                        x.Sum(y => y.Walks) +
                                        x.Sum(y => y.HitByPitch)
                                    )
                                ) -
                                (2 * x.Sum(y => y.Strikeouts))
                            )
                            / x.Sum(y => y.InningsPitched ?? 0)
                        ) + 3.10
                    )
            })
            .Where(x => !filters.OnlyQualifiedPlayers || (x.InningsPitched >= gamesPerSeason * 1 * x.NumSeasons))
            .OrderBy(orderBy)
            .Skip(((filters.PageNumber ?? 1) - 1) * limitValue)
            .Take(limitValue)
            .ToListAsync(cancellationToken: cancellationToken);

        var playerIds = playerPitchingDtos
            .Select(x => x.PlayerId)
            .Distinct()
            .ToList();
        var players = await _dbContext.Players
            .Where(x => playerIds.Contains(x.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var pitchingDto in playerPitchingDtos)
        {
            var player = players.Single(x => x.Id == pitchingDto.PlayerId);

            pitchingDto.PlayerName = $"{player.FirstName} {player.LastName}";
            pitchingDto.IsPitcher = player.PitcherRoleId != null;
            pitchingDto.PitcherRoleId = player.PitcherRoleId;
            pitchingDto.BatHandednessId = player.BatHandednessId;
            pitchingDto.ThrowHandednessId = player.ThrowHandednessId;
            pitchingDto.PrimaryPositionId = player.PrimaryPositionId;
            pitchingDto.ChemistryId = player.ChemistryId;
            pitchingDto.IsHallOfFamer = player.IsHallOfFamer;
            pitchingDto.IsRetired = pitchingDto.EndSeasonNumber < mostRecentSeason.Number;
            {
                pitchingDto.RetiredCurrentAge = pitchingDto.Age + (mostRecentSeason.Number - pitchingDto.EndSeasonNumber);
            }

            if (pitchingDto.NumChampionships > 0)
                foreach (var _ in Enumerable.Range(1, pitchingDto.NumChampionships))
                {
                    pitchingDto.AwardIds.Add((int) VirtualAward.Champion);
                }

            if (pitchingDto.IsHallOfFamer) pitchingDto.AwardIds.Add((int) VirtualAward.HallOfFame);
        }

        return playerPitchingDtos;
    }

    public async Task<List<PlayerCareerPitchingDto>> GetHallOfFameCandidates(int seasonId,
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
            return new List<PlayerCareerPitchingDto>();

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
            pitchingDto.EarnedRunAverage = pitchingDto.InningsPitched == 0
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
                    pitchingDto.AwardIds.Add((int) VirtualAward.Champion);
                }
        }

        return pitchingDtos
            .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
            .ToList();
    }

    public async Task<List<SimilarPlayerDto>> GetSimilarPitchingCareers(int playerId,
        CancellationToken cancellationToken = default)
    {
        var pitchingCareers = await GetPitchingCareers(
            new GetPitchingCareersFilters
            {
                PlayerId = playerId
            },
            cancellationToken);

        if (!pitchingCareers.Any())
            throw new Exception($"No player career found for player ID {playerId}");

        var playerCareerPitching = pitchingCareers.First();

        var wins = playerCareerPitching.Wins;
        var losses = playerCareerPitching.Losses;
        var era = playerCareerPitching.EarnedRunAverage;
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
                        .Sum(y => y.InningsPitched ?? 0) > 0
                        ? x.PlayerSeasons
                              .SelectMany(y => y.PitchingStats)
                              .Sum(y => (y.EraMinus ?? 0) * (y.InningsPitched ?? 0))
                          /
                          x.PlayerSeasons
                              .SelectMany(y => y.PitchingStats)
                              .Sum(y => y.InningsPitched ?? 0)
                        : 0,
                FipMinus =
                    x.PlayerSeasons
                        .SelectMany(y => y.PitchingStats)
                        .Sum(y => y.InningsPitched ?? 0) > 0
                        ? x.PlayerSeasons
                              .SelectMany(y => y.PitchingStats)
                              .Sum(y => (y.FipMinus ?? 0) * (y.InningsPitched ?? 0))
                          /
                          x.PlayerSeasons
                              .SelectMany(y => y.PitchingStats)
                              .Sum(y => y.InningsPitched ?? 0)
                        : 0,
                AwardIds = x.PlayerSeasons
                    .SelectMany(y => y.Awards)
                    .Where(y => !omitRunnerUps || !y.OmitFromGroupings)
                    .Select(y => y.Id)
                    .ToList(),
                IsHallOfFamer = x.IsHallOfFamer,
                NumChampionships = x.PlayerSeasons
                    .Count(y => y.ChampionshipWinner != null)
            });
    }
}