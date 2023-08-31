using System.Diagnostics;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Entities;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services;

public class PlayerRepository : IPlayerRepository
{
    private static readonly string[] PositiveFieldingTraitNames = {"Cannon Arm", "Dive Wizard", "Utility", "Magic Hands"};
    private static readonly string[] NegativeFieldingTraitNames = {"Butter Fingers", "Noodle Arm", "Wild Thrower"};
    private readonly IApplicationContext _applicationContext;
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PlayerRepository(SmbExplorerCompanionDbContext dbContext, IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    public async Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var playerCareerBattingResult = await GetBattingCareers(playerId: playerId, cancellationToken: cancellationToken);

            if (playerCareerBattingResult.TryPickT1(out var exception, out var playerCareerBattingDtos))
                return exception;

            var playerCareerPitchingResult = await GetPitchingCareers(playerId: playerId, cancellationToken: cancellationToken);

            if (playerCareerPitchingResult.TryPickT1(out exception, out var playerCareerPitchingDtos))
                return exception;

            var playerOverview = await GetPlayerOverview(playerId, cancellationToken);

            if (playerCareerBattingDtos.Any()) playerOverview.CareerBatting = playerCareerBattingDtos.First();

            if (playerCareerPitchingDtos.Any()) playerOverview.CareerPitching = playerCareerPitchingDtos.First();

            var playerSeasonBatting = await GetBattingSeasons(playerId: playerId, cancellationToken: cancellationToken);

            if (playerSeasonBatting.TryPickT1(out exception, out var playerSeasonBattingDtos))
                return exception;

            playerOverview.PlayerSeasonBatting = playerSeasonBattingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var playerSeasonPitching = await GetPitchingSeasons(playerId: playerId, cancellationToken: cancellationToken);

            if (playerSeasonPitching.TryPickT1(out exception, out var playerSeasonPitchingDtos))
                return exception;

            playerOverview.PlayerSeasonPitching = playerSeasonPitchingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var playerPlayoffBatting = await GetBattingSeasons(playerId: playerId, isPlayoffs: true, cancellationToken: cancellationToken);

            if (playerPlayoffBatting.TryPickT1(out exception, out var playerPlayoffBattingDtos))
                return exception;

            playerOverview.PlayerPlayoffBatting = playerPlayoffBattingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var playerPlayoffPitching = await GetPitchingSeasons(playerId: playerId, isPlayoffs: true, cancellationToken: cancellationToken);

            if (playerPlayoffPitching.TryPickT1(out exception, out var playerPlayoffPitchingDtos))
                return exception;

            playerOverview.PlayerPlayoffPitching = playerPlayoffPitchingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var gameStats = await _dbContext.PlayerSeasonGameStats
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Season)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.SeasonTeamHistory)
                .ThenInclude(x => x!.TeamNameHistory)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.SecondaryPosition)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Traits)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.PitchTypes)
                .Where(x => x.PlayerSeason.PlayerId == playerId)
                .Select(x => new PlayerGameStatOverviewDto
                {
                    SeasonNumber = x.PlayerSeason.Season.Number,
                    Age = x.PlayerSeason.Age,
                    TeamNames = string.Join(", ",
                        x.PlayerSeason.PlayerTeamHistory
                            .OrderBy(y => y.Order)
                            .Where(y => y.SeasonTeamHistory != null)
                            .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                    Power = x.Power,
                    Contact = x.Contact,
                    Speed = x.Speed,
                    Fielding = x.Fielding,
                    Arm = x.Arm,
                    Velocity = x.Velocity,
                    Junk = x.Junk,
                    Accuracy = x.Accuracy,
                    Salary = x.PlayerSeason.PlayerTeamHistory
                        .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                        ? 0
                        : x.PlayerSeason.Salary,
                    SecondaryPosition = x.PlayerSeason.SecondaryPosition == null ? null : x.PlayerSeason.SecondaryPosition.Name,
                    Traits = string.Join(", ", x.PlayerSeason.Traits.OrderBy(y => y.Id).Select(y => y.Name)),
                    PitchTypes = string.Join(", ", x.PlayerSeason.PitchTypes.OrderBy(y => y.Id).Select(y => y.Name))
                })
                .OrderByDescending(x => x.SeasonNumber)
                .ToListAsync(cancellationToken: cancellationToken);

            playerOverview.GameStats = gameStats;

            return playerOverview;
        }
        catch (Exception e)
        {
            return e;
        }
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
                    .SingleOrDefaultAsync(x => x.Id == primaryPositionId, cancellationToken: cancellationToken);
                
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
                if (battingDto.IsRetired)
                {
                    battingDto.RetiredCurrentAge = battingDto.Age + (mostRecentSeason.Number - battingDto.EndSeasonNumber);
                }
                
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
                {
                    battingDto.Awards.Add(new PlayerAwardBaseDto
                    {
                        Id = -1,
                        Name = "Hall of Fame",
                        Importance = 0,
                        OmitFromGroupings = false
                    });
                }
            }

            return playerBattingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
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
                    .SingleOrDefaultAsync(x => x.Id == pitcherRoleId, cancellationToken: cancellationToken);
                
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
                {
                    pitchingDto.Awards.Add(new PlayerAwardBaseDto
                    {
                        Id = -1,
                        Name = "Hall of Fame",
                        Importance = 0,
                        OmitFromGroupings = false
                    });
                }
            }

            return playerPitchingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
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
                    IsPitcher = x.PlayerSeason.Player.PitcherRole != null,
                    TotalSalary = x.PlayerSeason.PlayerTeamHistory
                        .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                        ? 0
                        : x.PlayerSeason.Salary,
                    BatHandedness = x.PlayerSeason.Player.BatHandedness.Name,
                    ThrowHandedness = x.PlayerSeason.Player.ThrowHandedness.Name,
                    PrimaryPosition = x.PlayerSeason.Player.PrimaryPosition.Name,
                    PitcherRole = x.PlayerSeason.Player.PitcherRole != null ? x.PlayerSeason.Player.PitcherRole.Name : null,
                    Chemistry = x.PlayerSeason.Player.Chemistry!.Name,
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
                    WeightedOpsPlusOrEraMinus = ((x.OpsPlus ?? 0) - 90) * x.PlateAppearances * BattingScalingFactor,
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
                    SecondaryPosition = x.PlayerSeason.SecondaryPosition == null ? null : x.PlayerSeason.SecondaryPosition.Name,
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

    public async Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> GetPitchingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false,
        int? playerId = null,
        int? pitcherRoleId = null,
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
            const string defaultOrderByProperty = nameof(PlayerPitchingSeasonDto.WeightedOpsPlusOrEraMinus);
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
                .Where(x => seasonId == null || x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.IsRegularSeason == !isPlayoffs)
                .Where(x => playerId == null || x.PlayerSeason.PlayerId == playerId)
                .Where(x => x.PlayerSeason.PlayerTeamHistory.Any(y => !limitToTeam ||
                                                                      (y.SeasonTeamHistory != null && y.SeasonTeamHistory.TeamId == teamId)))
                .Where(x => pitcherRoleId == null || x.PlayerSeason.Player.PitcherRoleId == pitcherRoleId)
                .Where(x => !onlyRookies || rookiePlayerIds.Contains(x.PlayerSeason.PlayerId))
                .Select(x => new PlayerPitchingSeasonDto
                {
                    PlayerId = x.PlayerSeason.PlayerId,
                    PlayerName = $"{x.PlayerSeason.Player.FirstName} {x.PlayerSeason.Player.LastName}",
                    TeamNames = string.Join(", ",
                        x.PlayerSeason.PlayerTeamHistory
                            .OrderBy(y => y.Order)
                            .Where(y => y.SeasonTeamHistory != null)
                            .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                    IsPitcher = x.PlayerSeason.Player.PitcherRole != null,
                    TotalSalary = x.PlayerSeason.PlayerTeamHistory
                        .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                        ? 0
                        : x.PlayerSeason.Salary,
                    BatHandedness = x.PlayerSeason.Player.BatHandedness.Name,
                    ThrowHandedness = x.PlayerSeason.Player.ThrowHandedness.Name,
                    PrimaryPosition = x.PlayerSeason.Player.PrimaryPosition.Name,
                    PitcherRole = x.PlayerSeason.Player.PitcherRole != null ? x.PlayerSeason.Player.PitcherRole.Name : null,
                    Chemistry = x.PlayerSeason.Player.Chemistry!.Name,
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
                    WeightedOpsPlusOrEraMinus = ((x.EraMinus ?? 0) - 90) * (x.InningsPitched ?? 0) * PitchingScalingFactor,
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
                    Traits = string.Join(", ", x.PlayerSeason.Traits.OrderBy(y => y.Id).Select(y => y.Name)),
                    Age = x.PlayerSeason.Age,
                    Era = x.EarnedRunAverage ?? 0,
                    GamesFinished = x.GamesFinished,
                    BattersFaced = x.BattersFaced,
                    HitsPerNine = x.HitsPerNine ?? 0,
                    HomeRunsPerNine = x.HomeRunsPerNine ?? 0,
                    WalksPerNine = x.WalksPerNine ?? 0,
                    StrikeoutsPerNine = x.StrikeoutsPerNine ?? 0,
                    StrikeoutToWalkRatio = x.StrikeoutsPerWalk ?? 0,
                    PitchTypes = string.Join(", ", x.PlayerSeason.PitchTypes.OrderBy(y => y.Id).Select(y => y.Name))
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * limitValue)
                .Take(limitValue)
                .ToListAsync(cancellationToken: cancellationToken);

            if (includeChampionAwards)
                foreach (var player in playerPitchingDtos.Where(player => player.IsChampion))
                {
                    player.Awards.Add(new PlayerAwardBaseDto
                    {
                        Id = 0,
                        Name = "Champion",
                        Importance = 10,
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

    public async Task<OneOf<List<PlayerFieldingRankingDto>, Exception>> GetPlayerFieldingRankings(int seasonId,
        int? primaryPositionId,
        int? pageNumber,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var limitValue = limit ?? 10;

        try
        {
            var positiveFieldingTraits = await _dbContext.Traits
                .Where(x => PositiveFieldingTraitNames.Contains(x.Name))
                .ToListAsync(cancellationToken: cancellationToken);
            Debug.Assert(positiveFieldingTraits.Count == PositiveFieldingTraitNames.Length, "Not all positive fielding traits were found.");

            var negativeFieldingTraits = await _dbContext.Traits
                .Where(x => NegativeFieldingTraitNames.Contains(x.Name))
                .ToListAsync(cancellationToken: cancellationToken);
            Debug.Assert(negativeFieldingTraits.Count == NegativeFieldingTraitNames.Length, "Not all negative fielding traits were found.");

            var playerGameStatDtos = await _dbContext.PlayerSeasonGameStats
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.SeasonTeamHistory)
                .ThenInclude(x => x!.TeamNameHistory)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Traits)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .ThenInclude(x => x.PrimaryPosition)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .ThenInclude(x => x.PitcherRole)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.BattingStats)
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.PitchingStats)
                .Where(x => x.PlayerSeason.SeasonId == seasonId)
                .Where(x => primaryPositionId == null || x.PlayerSeason.Player.PrimaryPositionId == primaryPositionId)
                .Select(x => new PlayerFieldingRankingDto
                {
                    PlayerId = x.PlayerSeason.PlayerId,
                    PlayerName = $"{x.PlayerSeason.Player.FirstName} {x.PlayerSeason.Player.LastName}",
                    TeamNames = string.Join(", ",
                        x.PlayerSeason.PlayerTeamHistory
                            .OrderBy(y => y.Order)
                            .Where(y => y.SeasonTeamHistory != null)
                            .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                    PrimaryPosition = x.PlayerSeason.Player.PrimaryPosition.Name,
                    SeasonId = x.PlayerSeason.SeasonId,
                    Speed = x.Speed,
                    Fielding = x.Fielding,
                    Arm = x.Arm,
                    PlateAppearances = x.PlayerSeason.BattingStats.Sum(y => y.PlateAppearances),
                    InningsPitched = x.PlayerSeason.PitchingStats.Sum(y => y.InningsPitched),
                    Errors = x.PlayerSeason.BattingStats.Sum(y => y.Errors),
                    PassedBalls = x.PlayerSeason.BattingStats.Sum(y => y.PassedBalls),
                    PositiveFieldingTraits = x.PlayerSeason.Traits
                        .Where(y => positiveFieldingTraits.Contains(y))
                        .Select(y => y.Name)
                        .ToList(),
                    NegativeFieldingTraits = x.PlayerSeason.Traits
                        .Where(y => negativeFieldingTraits.Contains(y))
                        .Select(y => y.Name)
                        .ToList(),
                    WeightedFieldingRanking =
                        x.Fielding + x.Speed + (x.Arm ?? 0) +
                        x.PlayerSeason.Traits.Count(y => positiveFieldingTraits.Contains(y)) * 20 -
                        x.PlayerSeason.Traits.Count(y => negativeFieldingTraits.Contains(y)) * 20 +
                        (x.PlayerSeason.Player.PitcherRoleId != null
                            ? x.PlayerSeason.PitchingStats.Sum(y => y.InningsPitched ?? 0)
                            : x.PlayerSeason.BattingStats.Sum(y => y.PlateAppearances)) /
                        (x.PlayerSeason.BattingStats.Sum(y => y.Errors) * 2 +
                         x.PlayerSeason.BattingStats.Sum(y => y.PassedBalls) * 1.5)
                })
                .OrderByDescending(x => x.WeightedFieldingRanking)
                .Skip(((pageNumber ?? 1) - 1) * limitValue)
                .Take(limitValue)
                .ToListAsync(cancellationToken: cancellationToken);

            return playerGameStatDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<RetiredPlayerCareerStatsDto, None, Exception>> GetHallOfFameCandidates(int seasonId,
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

            var battingDtos = await GetCareerBattingDtos(battingQueryable)
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
            
            var pitchingQueryable = GetCareerPitchingIQueryable()
                .Where(x => x.PitcherRoleId != null)
                .Where(x => retiredPlayers.Contains(x.Id));

            var pitchingDtos = await GetCareerPitchingDtos(pitchingQueryable)
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

            return new RetiredPlayerCareerStatsDto
            {
                BattingCareers = battingDtos
                    .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
                    .ToList(),
                PitchingCareers = pitchingDtos
                    .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
                    .ToList()
            };
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private async Task<PlayerOverviewDto> GetPlayerOverview(int playerId, CancellationToken cancellationToken)
    {
        var playerOverview = new PlayerOverviewDto();

        var player = await _dbContext.Players
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Awards)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Season)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.PlayerTeamHistory)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.ChampionshipWinner)
            .Include(x => x.Chemistry)
            .Include(x => x.ThrowHandedness)
            .Include(x => x.BatHandedness)
            .Include(x => x.PrimaryPosition)
            .Include(x => x.PitcherRole)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.BattingStats)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.PitchingStats)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.PlayerTeamHistory)
            .ThenInclude(x => x.SeasonTeamHistory)
            .ThenInclude(x => x!.TeamNameHistory)
            .Where(x => x.Id == playerId)
            .SingleAsync(cancellationToken: cancellationToken);

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;
        var mostRecentSeason = await _dbContext.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .OrderByDescending(x => x.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        playerOverview.PlayerId = player.Id;
        playerOverview.PlayerName = $"{player.FirstName} {player.LastName}";
        playerOverview.IsHallOfFamer = player.IsHallOfFamer;
        playerOverview.IsPitcher = player.PitcherRole is not null;
        playerOverview.TotalSalary = player.PlayerSeasons
            .Sum(x => x.PlayerTeamHistory
                .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                ? 0
                : x.Salary);
        playerOverview.BatHandedness = player.BatHandedness.Name;
        playerOverview.ThrowHandedness = player.ThrowHandedness.Name;
        playerOverview.PrimaryPosition = player.PrimaryPosition.Name;
        playerOverview.PitcherRole = player.PitcherRole?.Name;
        playerOverview.Chemistry = player.Chemistry!.Name;
        playerOverview.NumSeasons = player.PlayerSeasons.Count;
        playerOverview.Awards = player.PlayerSeasons
            .SelectMany(x => x.Awards)
            .Where(x => !x.OmitFromGroupings)
            .Select(x => new PlayerAwardDto
            {
                Id = x.Id,
                Name = x.Name,
                Importance = x.Importance,
                OmitFromGroupings = x.OmitFromGroupings,
                OriginalName = x.OriginalName,
                IsBattingAward = x.IsBattingAward,
                IsBuiltIn = x.IsBuiltIn,
                IsFieldingAward = x.IsFieldingAward,
                IsPitchingAward = x.IsPitchingAward,
                IsPlayoffAward = x.IsPlayoffAward,
                IsUserAssignable = x.IsUserAssignable
            })
            .ToList();
        playerOverview.NumChampionships = player.PlayerSeasons
            .Count(x => x.ChampionshipWinner is not null);

        if (player.IsHallOfFamer)
            playerOverview.Awards.Add(new PlayerAwardDto
            {
                Id = -1,
                Importance = -1,
                Name = "Hall of Fame",
                OriginalName = "Hall of Fame",
                OmitFromGroupings = false
            });

        if (playerOverview.NumChampionships > 0)
            foreach (var _ in Enumerable.Range(1, playerOverview.NumChampionships))
            {
                playerOverview.Awards.Add(new PlayerAwardDto
                {
                    Id = 0,
                    Name = "Champion",
                    Importance = 10,
                    OmitFromGroupings = false
                });
            }

        var startSeason = player.PlayerSeasons.MinBy(x => x.SeasonId)!.Season;
        var endPlayerSeason = player.PlayerSeasons.MaxBy(x => x.SeasonId)!;
        var endSeason = endPlayerSeason.Season;
        playerOverview.StartSeasonNumber = startSeason.Number;
        playerOverview.EndSeasonNumber = endSeason.Number;
        playerOverview.IsRetired = endSeason.Number < mostRecentSeason.Number;
        var currentTeam = endPlayerSeason.PlayerTeamHistory
            .OrderBy(x => x.Order)
            .LastOrDefault(x => x.SeasonTeamHistory != null)?
            .SeasonTeamHistory?.TeamNameHistory;

        playerOverview.CurrentTeam = currentTeam is null ? "Free Agent" : currentTeam.Name;
        playerOverview.CurrentTeamId = currentTeam?.Id;

        var weightedOpsPlus = player.PlayerSeasons
            .SelectMany(y => y.BattingStats)
            .Where(y => y.OpsPlus is not null)
            .Sum(y => ((y.OpsPlus ?? 0) - 90) * y.PlateAppearances * BattingScalingFactor);

        var weightedEraMinus = player.PlayerSeasons
            .SelectMany(y => y.PitchingStats)
            .Where(y => y is {EraMinus: not null, InningsPitched: not null})
            .Sum(y => ((y.EraMinus ?? 0) - 90) * (y.InningsPitched ?? 0) * PitchingScalingFactor);

        var weightedOpsPlusOrEraMinus = weightedOpsPlus + weightedEraMinus;
        playerOverview.WeightedOpsPlusOrEraMinus = weightedOpsPlusOrEraMinus;

        return playerOverview;
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

    private IQueryable<Player> GetCareerPitchingIQueryable()
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
            .ThenInclude(x => x.PitchingStats)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Awards)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.ChampionshipWinner);
    }

    private static IQueryable<PlayerCareerBattingDto> GetCareerBattingDtos(IQueryable<Player> players)
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
                ThrowHandedness = x.ThrowHandedness.Name,
                PrimaryPosition = x.PrimaryPosition.Name,
                PitcherRole = x.PitcherRole != null ? x.PitcherRole.Name : null,
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
                    .Sum(y => ((y.OpsPlus ?? 0) - 90) * y.PlateAppearances * BattingScalingFactor),
                // Simply average the OPS+ values
                OpsPlus = x.PlayerSeasons
                    .SelectMany(y => y.BattingStats)
                    .Average(y => y.OpsPlus ?? 0),
                Singles = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Singles)),
                Doubles = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Doubles)),
                Triples = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Triples)),
                Walks = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Walks)),
                Strikeouts = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Strikeouts)),
                HitByPitch = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HitByPitch)),
                SacrificeHits = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.SacrificeHits)),
                SacrificeFlies = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.SacrificeFlies)),
                Errors = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Errors)),
                Awards = x.PlayerSeasons
                    .SelectMany(y => y.Awards)
                    .Where(y => !y.OmitFromGroupings)
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

    private static IQueryable<PlayerCareerPitchingDto> GetCareerPitchingDtos(IQueryable<Player> players)
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
                PitcherRole = x.PitcherRole != null ? x.PitcherRole.Name : null,
                BatHandedness = x.BatHandedness.Name,
                ThrowHandedness = x.ThrowHandedness.Name,
                PrimaryPosition = x.PrimaryPosition.Name,
                Chemistry = x.Chemistry!.Name,
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
                    .Sum(y => ((y.EraMinus ?? 0) - 90) * (y.InningsPitched ?? 0) * PitchingScalingFactor),
                // Simply average the ERA- values, only taking into account regular season games for this calculation
                EraMinus = x.PlayerSeasons
                    .SelectMany(y => y.PitchingStats)
                    .Average(y => y.EraMinus ?? 0),
                FipMinus = x.PlayerSeasons
                    .SelectMany(y => y.PitchingStats)
                    .Average(y => y.FipMinus ?? 0),
                Awards = x.PlayerSeasons
                    .SelectMany(y => y.Awards)
                    .Where(y => !y.OmitFromGroupings)
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