using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using System.Linq.Dynamic.Core;
using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Services;

public class PlayerRepository : IPlayerRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IApplicationContext _applicationContext;

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
            var playerCareerBattingResult = await GetTopBattingCareers(playerId: playerId, cancellationToken: cancellationToken);

            if (playerCareerBattingResult.TryPickT1(out var exception, out var playerCareerBattingDtos))
                return exception;

            var playerCareerPitchingResult = await GetTopPitchingCareers(playerId: playerId, cancellationToken: cancellationToken);

            if (playerCareerPitchingResult.TryPickT1(out exception, out var playerCareerPitchingDtos))
                return exception;

            var playerOverview = await GetPlayerOverview(playerId, cancellationToken);

            if (playerCareerBattingDtos.Any())
            {
                playerOverview.CareerBatting = playerCareerBattingDtos.First();
            }
            
            if (playerCareerPitchingDtos.Any())
            {
                playerOverview.CareerPitching = playerCareerPitchingDtos.First();
            }

            var playerSeasonBatting = await GetTopBattingSeasons(playerId: playerId, cancellationToken: cancellationToken);

            if (playerSeasonBatting.TryPickT1(out exception, out var playerSeasonBattingDtos))
                return exception;

            playerOverview.PlayerSeasonBatting = playerSeasonBattingDtos;

            var playerSeasonPitching = await GetTopPitchingSeasons(playerId: playerId, cancellationToken: cancellationToken);

            if (playerSeasonPitching.TryPickT1(out exception, out var playerSeasonPitchingDtos))
                return exception;

            playerOverview.PlayerSeasonPitching = playerSeasonPitchingDtos;

            var playerPlayoffBatting = await GetTopBattingSeasons(playerId: playerId, isPlayoffs: true, cancellationToken: cancellationToken);

            if (playerPlayoffBatting.TryPickT1(out exception, out var playerPlayoffBattingDtos))
                return exception;

            playerOverview.PlayerPlayoffBatting = playerPlayoffBattingDtos;

            var playerPlayoffPitching = await GetTopPitchingSeasons(playerId: playerId, isPlayoffs: true, cancellationToken: cancellationToken);

            if (playerPlayoffPitching.TryPickT1(out exception, out var playerPlayoffPitchingDtos))
                return exception;

            playerOverview.PlayerPlayoffPitching = playerPlayoffPitchingDtos;

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
                        .SingleOrDefault(y => y.Order == 1) == null
                        ? 0
                        : x.PlayerSeason.Salary,
                    SecondaryPosition = x.PlayerSeason.SecondaryPosition == null ? null : x.PlayerSeason.SecondaryPosition.Name,
                    Traits = string.Join(", ", x.PlayerSeason.Traits.OrderBy(y => y.Id).Select(y => y.Name)),
                    PitchTypes = string.Join(", ", x.PlayerSeason.PitchTypes.OrderBy(y => y.Id).Select(y => y.Name))
                })
                .ToListAsync(cancellationToken: cancellationToken);

            playerOverview.GameStats = gameStats;

            return playerOverview;
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
                .SingleOrDefault(y => y.Order == 1) == null
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
            .Where(y => y.OpsPlus != null)
            .Sum(y => (y.OpsPlus ?? 0) * y.AtBats / 10000);

        var weightedEraMinus = player.PlayerSeasons
            .SelectMany(y => y.PitchingStats)
            .Where(y => y.EraMinus != null && y.InningsPitched != null)
            .Sum(y => (y.EraMinus ?? 0) * (y.InningsPitched ?? 0) * 2.25 / 10000);

        var weightedOpsPlusOrEraMinus = weightedOpsPlus + weightedEraMinus;
        playerOverview.WeightedOpsPlusOrEraMinus = weightedOpsPlusOrEraMinus;

        return playerOverview;
    }

    public async Task<OneOf<List<PlayerCareerBattingDto>, Exception>> GetTopBattingCareers(
        int? pageNumber = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        CancellationToken cancellationToken = default)
    {
        if (playerId is not null && pageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

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
            var mostRecentSeason = await _dbContext.Seasons
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.Team)
                .Where(x => x.SeasonTeamHistory.First().Team.FranchiseId == franchiseId)
                .OrderByDescending(x => x.Id)
                .FirstAsync(cancellationToken);

            var playerCareerDtos = await _dbContext.Players
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
                .ThenInclude(x => x.ChampionshipWinner)
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => playerId == null || x.Id == playerId)
                .Select(x => new PlayerCareerBattingDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    IsPitcher = x.PitcherRole != null,
                    TotalSalary = x.PlayerSeasons
                        .Sum(y => y.PlayerTeamHistory
                            .SingleOrDefault(z => z.Order == 1) == null
                            ? 0
                            : y.Salary),
                    BatHandedness = x.BatHandedness.Name,
                    ThrowHandedness = x.ThrowHandedness.Name,
                    PrimaryPosition = x.PrimaryPosition.Name,
                    Chemistry = x.Chemistry!.Name,
                    StartSeasonNumber = x.PlayerSeasons.Min(y => y.Season.Number),
                    EndSeasonNumber = x.PlayerSeasons.Max(y => y.Season.Number),
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
                        .Where(y => y.OpsPlus != null)
                        .Sum(y => (y.OpsPlus ?? 0) * y.AtBats / 10000),
                    // Simply average the OPS+ values
                    OpsPlus = x.PlayerSeasons
                        .SelectMany(y => y.BattingStats)
                        .Where(y => y.OpsPlus != null && y.IsRegularSeason)
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
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * 30)
                .Take(30)
                .ToListAsync(cancellationToken: cancellationToken);

            // Calculate the rate stats that we omitted above
            foreach (var playerCareerDto in playerCareerDtos)
            {
                playerCareerDto.IsRetired = playerCareerDto.EndSeasonNumber < mostRecentSeason.Number;
                playerCareerDto.BattingAverage = playerCareerDto.AtBats == 0
                    ? 0
                    : playerCareerDto.Hits / (double) playerCareerDto.AtBats;
                playerCareerDto.Obp = playerCareerDto.AtBats == 0
                    ? 0
                    : (playerCareerDto.Hits + playerCareerDto.Walks + playerCareerDto.HitByPitch) /
                      (double) (playerCareerDto.AtBats + playerCareerDto.Walks + playerCareerDto.HitByPitch +
                                playerCareerDto.SacrificeFlies);
                playerCareerDto.Slg = playerCareerDto.AtBats == 0
                    ? 0
                    : (playerCareerDto.Singles + playerCareerDto.Doubles * 2 + playerCareerDto.Triples * 3 +
                       playerCareerDto.HomeRuns * 4) / (double) playerCareerDto.AtBats;
                playerCareerDto.Ops = playerCareerDto.Obp + playerCareerDto.Slg;

                if (playerCareerDto.NumChampionships > 0)
                {
                    foreach (var i in Enumerable.Range(1, playerCareerDto.NumChampionships))
                    {
                        playerCareerDto.Awards.Add(new PlayerAwardBaseDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }
                }
            }

            return playerCareerDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerCareerPitchingDto>, Exception>> GetTopPitchingCareers(
        int? pageNumber = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        CancellationToken cancellationToken = default)
    {
        if (playerId is not null && pageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

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
            var mostRecentSeason = await _dbContext.Seasons
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.Team)
                .Where(x => x.SeasonTeamHistory.First().Team.FranchiseId == franchiseId)
                .OrderByDescending(x => x.Id)
                .FirstAsync(cancellationToken);

            var playerCareerDtos = await _dbContext.Players
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
                .ThenInclude(x => x.ChampionshipWinner)
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => x.PitcherRole != null)
                .Where(x => playerId == null || x.Id == playerId)
                .Select(x => new PlayerCareerPitchingDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    IsPitcher = x.PitcherRole != null,
                    TotalSalary = x.PlayerSeasons
                        .Sum(y => y.PlayerTeamHistory
                            .SingleOrDefault(z => z.Order == 1) == null
                            ? 0
                            : y.Salary),
                    PitcherRole = x.PitcherRole != null ? x.PitcherRole.Name : null,
                    BatHandedness = x.BatHandedness.Name,
                    ThrowHandedness = x.ThrowHandedness.Name,
                    PrimaryPosition = x.PrimaryPosition.Name,
                    Chemistry = x.Chemistry!.Name,
                    StartSeasonNumber = x.PlayerSeasons.Min(y => y.Season.Number),
                    EndSeasonNumber = x.PlayerSeasons.Max(y => y.Season.Number),
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
                        .Where(y => y.EraMinus != null && y.InningsPitched != null)
                        .Sum(y => (y.EraMinus ?? 0) * (y.InningsPitched ?? 0) * 2.25 / 10000),
                    // Simply average the ERA- values, only taking into account regular season games for this calculation
                    EraMinus = x.PlayerSeasons
                        .SelectMany(y => y.PitchingStats)
                        .Where(y => y.EraMinus != null && y.IsRegularSeason)
                        .Average(y => y.EraMinus ?? 0),
                    FipMinus = x.PlayerSeasons
                        .SelectMany(y => y.PitchingStats)
                        .Where(y => y.FipMinus != null && y.IsRegularSeason)
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
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * 30)
                .Take(30)
                .ToListAsync(cancellationToken: cancellationToken);

            foreach (var playerCareerDto in playerCareerDtos)
            {
                playerCareerDto.IsRetired = playerCareerDto.EndSeasonNumber < mostRecentSeason.Number;
                playerCareerDto.Era = playerCareerDto.InningsPitched == 0
                    ? 0
                    : playerCareerDto.EarnedRuns / playerCareerDto.InningsPitched * 9;
                playerCareerDto.Whip = playerCareerDto.InningsPitched == 0
                    ? 0
                    : (playerCareerDto.Walks + playerCareerDto.Hits) / playerCareerDto.InningsPitched;
                playerCareerDto.Fip = playerCareerDto.InningsPitched == 0
                    ? 0
                    : (13 * playerCareerDto.HomeRuns + 3 * (playerCareerDto.Walks + playerCareerDto.HitByPitch) -
                       2 * playerCareerDto.Strikeouts) / playerCareerDto.InningsPitched + 3.10;

                if (playerCareerDto.NumChampionships > 0)
                {
                    foreach (var i in Enumerable.Range(1, playerCareerDto.NumChampionships))
                    {
                        playerCareerDto.Awards.Add(new PlayerAwardBaseDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }
                }
            }

            return playerCareerDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> GetTopBattingSeasons(
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
                .MinAsync(x => x.Id, cancellationToken: cancellationToken);

            if (seasonId == minSeasonId) onlyRookies = false;

            List<int> rookiePlayerIds = new();
            if (onlyRookies)
            {
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
            }

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
                        .SingleOrDefault(y => y.Order == 1) == null
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
                    WeightedOpsPlusOrEraMinus = (x.OpsPlus ?? 0) * x.PlateAppearances / 10000,
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
                    Traits = string.Join(", ", x.PlayerSeason.Traits.OrderBy(y => y.Id).Select(y => y.Name)),
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * limitValue)
                .Take(limitValue)
                .ToListAsync(cancellationToken: cancellationToken);

            if (includeChampionAwards)
            {
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
            }

            return playerBattingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> GetTopPitchingSeasons(
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
                .MinAsync(x => x.Id, cancellationToken: cancellationToken);

            if (seasonId == minSeasonId) onlyRookies = false;

            List<int> rookiePlayerIds = new();
            if (onlyRookies)
            {
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
            }

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
                        .SingleOrDefault(y => y.Order == 1) == null
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
                    WeightedOpsPlusOrEraMinus = (x.EraMinus ?? 0) * (x.InningsPitched ?? 0) * 2.25 / 10000,
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
            {
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
            }

            return playerPitchingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private static readonly string[] PositiveFieldingTraitNames = {"Cannon Arm", "Dive Wizard", "Utility", "Magic Hands"};
    private static readonly string[] NegativeFieldingTraitNames = {"Butter Fingers", "Noodle Arm", "Wild Thrower"};

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
                        (x.PlayerSeason.Traits.Count(y => positiveFieldingTraits.Contains(y)) * 20) -
                        (x.PlayerSeason.Traits.Count(y => negativeFieldingTraits.Contains(y)) * 20) +
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
}