using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Shared.Enums;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services.Players;

public class PitcherSeasonRepository : IPitcherSeasonRepository
{
    private readonly IApplicationContext _applicationContext;
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PitcherSeasonRepository(SmbExplorerCompanionDbContext dbContext, IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    public async Task<List<PlayerPitchingSeasonDto>> GetPitchingSeasons(
        GetPitchingSeasonsFilters filters,
        CancellationToken cancellationToken = default)
    {
        if (filters is {OnlyRookies: true, Seasons: null})
            throw new ArgumentException("SeasonRange must be provided if OnlyRookies is true");

        if (filters.PlayerId is not null && filters.PageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        var limitToTeam = filters.TeamId is not null;
        var orderBy = filters.OrderBy;
        if (filters.OrderBy is not null)
        {
            orderBy += filters.Descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerPitchingSeasonDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        var limitValue = filters.Limit ?? 30;

        var startSeason = await _dbContext.Seasons
            .Where(x => x.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
            .OrderBy(x => x.Id)
            .FirstAsync(cancellationToken: cancellationToken);
        var minSeasonId = startSeason.Id;
        var gamesPerSeason = startSeason.NumGamesRegularSeason;

        var onlyRookies = filters.OnlyRookies;
        if (filters.Seasons?.StartSeasonId == minSeasonId ||
            filters.Seasons?.EndSeasonId > filters.Seasons?.StartSeasonId) onlyRookies = false;

        List<int> rookiePlayerIds = new();
        if (onlyRookies)
            rookiePlayerIds = await _dbContext.Players
                .Where(x => x.PlayerSeasons.Any(p => p.SeasonId == filters.Seasons!.Value.StartSeasonId))
                .Select(x => new
                {
                    PlayerId = x.Id,
                    FirstSeasonId = x.PlayerSeasons.OrderBy(ps => ps.SeasonId).First().SeasonId
                })
                .Where(x => x.FirstSeasonId == filters.Seasons!.Value.StartSeasonId)
                .Select(x => x.PlayerId)
                .ToListAsync(cancellationToken: cancellationToken);

        var hasTraitFilters = filters.TraitIds.Count > 0;

        var playerPitchingDtos = await _dbContext.PlayerSeasonPitchingStats
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Traits)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.PitchTypes)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Awards)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .ThenInclude(x => x.Chemistry)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .ThenInclude(x => x.BatHandedness)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .ThenInclude(x => x.ThrowHandedness)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .ThenInclude(x => x.PrimaryPosition)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .ThenInclude(x => x.PitcherRole)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Season)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.PlayerTeamHistory)
            .ThenInclude(x => x.SeasonTeamHistory)
            .ThenInclude(x => x!.TeamNameHistory)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.ChampionshipWinner)
            .Where(x => filters.Seasons == null || (x.PlayerSeason.SeasonId >= filters.Seasons.Value.StartSeasonId &&
                                                    x.PlayerSeason.SeasonId <= filters.Seasons.Value.EndSeasonId))
            .Where(x => x.IsRegularSeason == !filters.IsPlayoffs)
            .Where(x => filters.PlayerId == null || x.PlayerSeason.PlayerId == filters.PlayerId)
            .Where(x => x.PlayerSeason.PlayerTeamHistory.Any(y => !limitToTeam ||
                                                                  (y.SeasonTeamHistory != null && y.SeasonTeamHistory.TeamId == filters.TeamId)))
            .Where(x => filters.PitcherRoleId == null || x.PlayerSeason.Player.PitcherRoleId == filters.PitcherRoleId)
            .Where(x => !onlyRookies || rookiePlayerIds.Contains(x.PlayerSeason.PlayerId))
            .Where(x => filters.ChemistryId == null || x.PlayerSeason.Player.ChemistryId == filters.ChemistryId)
            .Where(x => filters.BatHandednessId == null || x.PlayerSeason.Player.BatHandednessId == filters.BatHandednessId)
            .Where(x => filters.ThrowHandednessId == null || x.PlayerSeason.Player.ThrowHandednessId == filters.ThrowHandednessId)
            .Where(x => !hasTraitFilters || x.PlayerSeason.Traits.Any(y => filters.TraitIds.Contains(y.Id)))
            .Where(x => !filters.OnlyQualifiedPlayers || x.InningsPitched >= gamesPerSeason * 1)
            .Select(x => new PlayerPitchingSeasonDto
            {
                PlayerId = x.PlayerSeason.PlayerId,
                PlayerName = $"{x.PlayerSeason.Player.FirstName} {x.PlayerSeason.Player.LastName}",
                TeamNames = string.Join(", ",
                    x.PlayerSeason.PlayerTeamHistory
                        .OrderBy(y => y.Order)
                        .Where(y => y.SeasonTeamHistory != null)
                        .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                IsPitcher = x.PlayerSeason.Player.PitcherRoleId != null,
                TotalSalary = x.PlayerSeason.PlayerTeamHistory
                    .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                    ? 0
                    : x.PlayerSeason.Salary,
                SeasonId = x.PlayerSeason.SeasonId,
                SeasonNumber = x.PlayerSeason.Season.Number,
                Wins = x.Wins,
                Losses = x.Losses,
                EarnedRuns = x.EarnedRuns,
                TotalPitches = x.TotalPitches,
                EarnedRunAverage = x.EarnedRunAverage ?? 0,
                Fip = x.Fip ?? 0,
                GamesStarted = x.GamesStarted,
                Saves = x.Saves,
                InningsPitched = x.InningsPitched ?? 0,
                Strikeouts = x.Strikeouts,
                Walks = x.Walks,
                Hits = x.Hits,
                HomeRuns = x.HomeRuns,
                HitByPitch = x.HitByPitch,
                Whip = x.Whip ?? 0,
                EraMinus = x.EraMinus ?? 0,
                FipMinus = x.FipMinus ?? 0,
                CompleteGames = x.CompleteGames,
                Shutouts = x.Shutouts,
                WeightedOpsPlusOrEraMinus =
                    (((x.EraMinus ?? 0) + (x.FipMinus ?? 0)) / 2 - 95) * (x.InningsPitched ?? 0) * PitchingScalingFactor,
                AwardIds = x.PlayerSeason.Awards
                    .Where(y => !filters.OnlyUserAssignableAwards || y.IsUserAssignable)
                    .Select(y => y.Id)
                    .ToList(),
                IsChampion = x.PlayerSeason.ChampionshipWinner != null,
                Age = x.PlayerSeason.Age,
                Era = x.EarnedRunAverage ?? 0,
                GamesFinished = x.GamesFinished,
                BattersFaced = x.BattersFaced,
                HitsPerNine = x.HitsPerNine ?? 0,
                HomeRunsPerNine = x.HomeRunsPerNine ?? 0,
                WalksPerNine = x.WalksPerNine ?? 0,
                StrikeoutsPerNine = x.StrikeoutsPerNine ?? 0,
                StrikeoutToWalkRatio = x.StrikeoutsPerWalk ?? 0,
                TraitIds = x.PlayerSeason.Traits.Select(y => y.Id).ToList(),
                PitchTypeIds = x.PlayerSeason.PitchTypes.Select(y => y.Id).ToList(),
                PrimaryPositionId = x.PlayerSeason.Player.PrimaryPositionId
            })
            .OrderBy(orderBy)
            .Skip(((filters.PageNumber ?? 1) - 1) * limitValue)
            .Take(limitValue)
            .ToListAsync(cancellationToken: cancellationToken);

        if (filters.IncludeChampionAwards)
            foreach (var player in playerPitchingDtos.Where(player => player.IsChampion))
            {
                player.AwardIds.Add((int) VirtualAward.Champion);
            }

        return playerPitchingDtos;
    }
}