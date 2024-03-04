using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services.Players;

public class PositionPlayerSeasonRepository : IPositionPlayerSeasonRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PositionPlayerSeasonRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> GetBattingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        int? primaryPositionId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false,
        int? playerId = null,
        CancellationToken cancellationToken = default)
    {
        if (onlyRookies && seasonId is null)
            throw new ArgumentException("SeasonId must be provided if OnlyRookies is true");

        if (playerId is not null && pageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        var limitToTeam = teamId is not null;
        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerBattingSeasonDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        var limitValue = limit ?? 30;

        try
        {
            var minSeasonId = await _dbContext.Seasons
                .MinAsync(x => x.Id, cancellationToken);

            if (seasonId == minSeasonId) onlyRookies = false;

            List<int> rookiePlayerIds = new();
            if (onlyRookies)
                rookiePlayerIds = await _dbContext.Players
                    .Where(x => x.PlayerSeasons.Any(p => p.SeasonId == seasonId))
                    .Select(x => new
                    {
                        PlayerId = x.Id,
                        FirstSeasonId = x.PlayerSeasons.OrderBy(ps => ps.SeasonId).First().SeasonId
                    })
                    .Where(x => x.FirstSeasonId == seasonId)
                    .Select(x => x.PlayerId)
                    .ToListAsync(cancellationToken: cancellationToken);

            var playerBattingDtos = await _dbContext.PlayerSeasonBattingStats
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.SecondaryPosition)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Traits)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Awards)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Season)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.SeasonTeamHistory)
                .ThenInclude(x => x!.TeamNameHistory)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.ChampionshipWinner)
                .Where(x => seasonId == null || x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.IsRegularSeason == !isPlayoffs)
                .Where(x => x.PlayerSeason.PlayerTeamHistory.Any(y => !limitToTeam ||
                                                                      (y.SeasonTeamHistory != null && y.SeasonTeamHistory.TeamId == teamId)))
                .Where(x => primaryPositionId == null || x.PlayerSeason.Player.PrimaryPositionId == primaryPositionId)
                .Where(x => !onlyRookies || rookiePlayerIds.Contains(x.PlayerSeason.PlayerId))
                .Where(x => playerId == null || x.PlayerSeason.PlayerId == playerId)
                .Select(x => new PlayerBattingSeasonDto
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
                    BatHandednessId = x.PlayerSeason.Player.BatHandednessId,
                    ThrowHandednessId = x.PlayerSeason.Player.ThrowHandednessId,
                    PrimaryPositionId = x.PlayerSeason.Player.PrimaryPositionId,
                    PitcherRoleId = x.PlayerSeason.Player.PitcherRoleId,
                    ChemistryId = x.PlayerSeason.Player.ChemistryId,
                    SeasonId = x.PlayerSeason.SeasonId,
                    SeasonNumber = x.PlayerSeason.Season.Number,
                    AtBats = x.AtBats,
                    Hits = x.Hits,
                    Singles = x.Singles,
                    Doubles = x.Doubles,
                    Triples = x.Triples,
                    HomeRuns = x.HomeRuns,
                    Walks = x.Walks,
                    BattingAverage = x.BattingAverage ?? 0,
                    Runs = x.Runs,
                    RunsBattedIn = x.RunsBattedIn,
                    StolenBases = x.StolenBases,
                    HitByPitch = x.HitByPitch,
                    SacrificeHits = x.SacrificeHits,
                    SacrificeFlies = x.SacrificeFlies,
                    Obp = x.Obp ?? 0,
                    Slg = x.Slg ?? 0,
                    Ops = x.Ops ?? 0,
                    OpsPlus = x.OpsPlus ?? 0,
                    Errors = x.Errors,
                    Strikeouts = x.Strikeouts,
                    WeightedOpsPlusOrEraMinus = ((x.OpsPlus ?? 0) - 95) * x.PlateAppearances * BattingScalingFactor +
                                                (x.StolenBases - x.CaughtStealing) * BaserunningScalingFactor,
                    Awards = x.PlayerSeason.Awards
                        .Where(y => !onlyUserAssignableAwards || y.IsUserAssignable)
                        .Select(y => new PlayerAwardBaseDto
                        {
                            Id = y.Id,
                            Name = y.Name,
                            Importance = y.Importance,
                            OmitFromGroupings = y.OmitFromGroupings
                        })
                        .ToList(),
                    IsChampion = x.PlayerSeason.ChampionshipWinner != null,
                    Age = x.PlayerSeason.Age,
                    GamesBatting = x.GamesBatting,
                    GamesPlayed = x.GamesPlayed,
                    PlateAppearances = x.PlateAppearances,
                    CaughtStealing = x.CaughtStealing,
                    TotalBases = x.TotalBases,
                    SecondaryPositionId = x.PlayerSeason.SecondaryPosition == null ? null : x.PlayerSeason.SecondaryPositionId,
                    Traits = string.Join(", ", x.PlayerSeason.Traits.OrderBy(y => y.Id).Select(y => y.Name))
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * limitValue)
                .Take(limitValue)
                .ToListAsync(cancellationToken: cancellationToken);

            if (includeChampionAwards)
                foreach (var player in playerBattingDtos.Where(player => player.IsChampion))
                {
                    player.Awards.Add(new PlayerAwardBaseDto
                    {
                        Id = 0,
                        Name = "Champion",
                        Importance = 10,
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
}