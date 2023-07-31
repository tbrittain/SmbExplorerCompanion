using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using System.Linq.Dynamic.Core;

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
            var playerOverviewDto = new PlayerOverviewDto();

            var playerWithSeasons = await _dbContext.Players
                .Include(x => x.Chemistry)
                .Include(x => x.PitcherRole)
                .Include(x => x.ThrowHandedness)
                .Include(x => x.BatHandedness)
                .Include(x => x.PrimaryPosition)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.GameStats)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.Season)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.BattingStats)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PitchingStats)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PitchTypes)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.Traits)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.SecondaryPosition)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.SeasonTeamHistory)
                .ThenInclude(x => x!.TeamNameHistory)
                .Where(x => x.Id == playerId)
                .SingleAsync(cancellationToken: cancellationToken);

            playerOverviewDto.PlayerId = playerWithSeasons.Id;
            playerOverviewDto.PlayerName = $"{playerWithSeasons.FirstName} {playerWithSeasons.LastName}";

            var currentTeam = playerWithSeasons.PlayerSeasons
                .OrderByDescending(x => x.Season.Number)
                .First()
                .PlayerTeamHistory
                .OrderByDescending(x => x.Order)
                .First()
                .SeasonTeamHistory;
            playerOverviewDto.CurrentTeam = currentTeam?.TeamNameHistory
                .Name ?? "Free Agent";
            playerOverviewDto.CurrentTeamId = currentTeam?.TeamId;
            playerOverviewDto.IsPitcher = playerWithSeasons.PitcherRole is not null;
            playerOverviewDto.BatHandedness = playerWithSeasons.BatHandedness.Name;
            playerOverviewDto.ThrowHandedness = playerWithSeasons.ThrowHandedness.Name;
            playerOverviewDto.PrimaryPosition = playerWithSeasons.PrimaryPosition.Name;
            playerOverviewDto.PitcherRole = playerWithSeasons.PitcherRole?.Name;
            // TODO: Asserting here because only supporting SMB4 for now
            playerOverviewDto.Chemistry = playerWithSeasons.Chemistry!.Name;
            playerOverviewDto.TotalSalary = playerWithSeasons.PlayerSeasons
                .Sum(x =>
                {
                    var mostRecentTeam = x.PlayerTeamHistory.First(y => y.Order == 1);
                    return mostRecentTeam.SeasonTeamHistory is null ? 0 : x.Salary;
                });

            // Batting specific stats
            playerOverviewDto.AtBats = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.AtBats));
            playerOverviewDto.Hits = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.Hits));
            playerOverviewDto.HomeRuns = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.HomeRuns));
            playerOverviewDto.BattingAverage = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.AtBats)) == 0
                ? 0
                : playerWithSeasons.PlayerSeasons
                      .Sum(x => x.BattingStats.Sum(y => y.Hits)) /
                  (double) playerWithSeasons.PlayerSeasons
                      .Sum(x => x.BattingStats.Sum(y => y.AtBats));
            playerOverviewDto.Runs = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.Runs));
            playerOverviewDto.RunsBattedIn = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.RunsBattedIn));
            playerOverviewDto.StolenBases = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.StolenBases));
            playerOverviewDto.Obp = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.PlateAppearances)) == 0
                ? 0
                : (playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Hits)) +
                   playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Walks)) +
                   playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.HitByPitch))) /
                  (double) (playerWithSeasons.PlayerSeasons
                                .Sum(x => x.BattingStats.Sum(y => y.AtBats)) +
                            playerWithSeasons.PlayerSeasons
                                .Sum(x => x.BattingStats.Sum(y => y.Walks)) +
                            playerWithSeasons.PlayerSeasons
                                .Sum(x => x.BattingStats.Sum(y => y.HitByPitch)) +
                            playerWithSeasons.PlayerSeasons
                                .Sum(x => x.BattingStats.Sum(y => y.SacrificeFlies)));
            playerOverviewDto.Slg = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.AtBats)) == 0
                ? 0
                : (playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Singles)) +
                   playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Doubles)) * 2 +
                   playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Triples)) * 3 +
                   playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.HomeRuns)) * 4) /
                  (double) playerWithSeasons.PlayerSeasons
                      .Sum(x => x.BattingStats.Sum(y => y.AtBats));
            playerOverviewDto.Ops = playerOverviewDto.Obp + playerOverviewDto.Slg;

            var battingStats = playerWithSeasons.PlayerSeasons
                .SelectMany(x => x.BattingStats)
                .ToList();

            if (battingStats.Any())
            {
                playerOverviewDto.OpsPlus = battingStats
                    .Where(x => x.OpsPlus is not null && x.IsRegularSeason)
                    .Average(x => x.OpsPlus!.Value);
            }
            else
            {
                playerOverviewDto.OpsPlus = 0;
            }

            // Pitching specific stats
            playerOverviewDto.Wins = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Wins));
            playerOverviewDto.Losses = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Losses));
            playerOverviewDto.Era = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched)) == 0
                ? 0
                : playerWithSeasons.PlayerSeasons
                    .Sum(x => x.PitchingStats.Sum(y => y.EarnedRuns)) /
                playerWithSeasons.PlayerSeasons
                    .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched.GetValueOrDefault())) * 9;
            playerOverviewDto.GamesPlayed = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.GamesPlayed));
            playerOverviewDto.GamesStarted = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.GamesStarted));
            playerOverviewDto.Saves = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Saves));
            playerOverviewDto.InningsPitched = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched ?? 0));
            playerOverviewDto.Strikeouts = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Strikeouts));
            playerOverviewDto.Whip = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched)) == 0
                ? 0
                : (playerWithSeasons.PlayerSeasons
                       .Sum(x => x.PitchingStats.Sum(y => y.Hits)) +
                   playerWithSeasons.PlayerSeasons
                       .Sum(x => x.PitchingStats.Sum(y => y.Walks))) /
                  playerWithSeasons.PlayerSeasons
                      .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched ?? 0));

            var pitchingStats = playerWithSeasons.PlayerSeasons
                .SelectMany(x => x.PitchingStats)
                .ToList();

            if (pitchingStats.Any())
            {
                playerOverviewDto.EraMinus = pitchingStats
                    .Where(x => x.EraMinus is not null && x.IsRegularSeason)
                    .Average(x => x.EraMinus!.Value);
            }
            else
            {
                playerOverviewDto.EraMinus = 0;
            }

            var battingSeasons = playerWithSeasons.PlayerSeasons
                .Select(x => new
                {
                    SeasonNumber = x.Season.Number,
                    TeamNames = string.Join(", ",
                        x.PlayerTeamHistory
                            .OrderBy(y => y.Order)
                            .Where(y => y.SeasonTeamHistory is not null)
                            .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                    Salary = x.PlayerTeamHistory
                        .SingleOrDefault(y => y.Order == 1) is null
                        ? 0
                        : x.Salary,
                    SecondaryPosition = x.SecondaryPosition?.Name,
                    Traits = x.Traits.Any() ? string.Join(", ", x.Traits.OrderBy(y => y.Id).Select(y => y.Name)) : string.Empty,
                    x.Age,
                    x.BattingStats
                })
                .ToList();

            var pitchingSeasons = playerWithSeasons.PlayerSeasons
                .Select(x => new
                {
                    SeasonNumber = x.Season.Number,
                    TeamNames = string.Join(", ",
                        x.PlayerTeamHistory
                            .OrderBy(y => y.Order)
                            .Where(y => y.SeasonTeamHistory is not null)
                            .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                    Salary = x.PlayerTeamHistory
                        .SingleOrDefault(y => y.Order == 1) is null
                        ? 0
                        : x.Salary,
                    Traits = x.Traits.Any() ? string.Join(", ", x.Traits.OrderBy(y => y.Id).Select(y => y.Name)) : string.Empty,
                    PitchTypes = x.PitchTypes.Any() ? string.Join(", ", x.PitchTypes.OrderBy(y => y.Id).Select(y => y.Name)) : string.Empty,
                    x.Age,
                    x.PitchingStats
                })
                .ToList();

            var seasonBatting = battingSeasons
                .Select(x =>
                {
                    var battingStat = x.BattingStats.SingleOrDefault(y => y.IsRegularSeason);
                    if (battingStat is null) return null;

                    return new PlayerBattingOverviewDto
                    {
                        SeasonNumber = x.SeasonNumber,
                        Age = x.Age,
                        TeamNames = x.TeamNames,
                        Salary = x.Salary,
                        SecondaryPosition = x.SecondaryPosition,
                        Traits = x.Traits,
                        Games = battingStat.GamesBatting,
                        PlateAppearances = battingStat.PlateAppearances,
                        AtBats = battingStat.AtBats,
                        Runs = battingStat.Runs,
                        Hits = battingStat.Hits,
                        Singles = battingStat.Singles,
                        Doubles = battingStat.Doubles,
                        Triples = battingStat.Triples,
                        HomeRuns = battingStat.HomeRuns,
                        RunsBattedIn = battingStat.RunsBattedIn,
                        StolenBases = battingStat.StolenBases,
                        CaughtStealing = battingStat.CaughtStealing,
                        Walks = battingStat.Walks,
                        Strikeouts = battingStat.Strikeouts,
                        BattingAverage = battingStat.BattingAverage ?? 0,
                        Obp = battingStat.Obp ?? 0,
                        Slg = battingStat.Slg ?? 0,
                        Ops = battingStat.Ops ?? 0,
                        OpsPlus = battingStat.OpsPlus ?? 0,
                        TotalBases = battingStat.TotalBases,
                        HitByPitch = battingStat.HitByPitch,
                        SacrificeHits = battingStat.SacrificeHits,
                        SacrificeFlies = battingStat.SacrificeFlies,
                        Errors = battingStat.Errors,
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            playerOverviewDto.PlayerSeasonBatting.AddRange(seasonBatting);

            var playoffBatting = battingSeasons
                .Select(x =>
                {
                    var battingStat = x.BattingStats.SingleOrDefault(y => !y.IsRegularSeason);
                    if (battingStat is null) return null;

                    return new PlayerBattingOverviewDto
                    {
                        SeasonNumber = x.SeasonNumber,
                        Age = x.Age,
                        TeamNames = x.TeamNames,
                        Salary = x.Salary,
                        SecondaryPosition = x.SecondaryPosition,
                        Traits = x.Traits,
                        Games = battingStat.GamesBatting,
                        PlateAppearances = battingStat.PlateAppearances,
                        AtBats = battingStat.AtBats,
                        Runs = battingStat.Runs,
                        Hits = battingStat.Hits,
                        Singles = battingStat.Singles,
                        Doubles = battingStat.Doubles,
                        Triples = battingStat.Triples,
                        HomeRuns = battingStat.HomeRuns,
                        RunsBattedIn = battingStat.RunsBattedIn,
                        StolenBases = battingStat.StolenBases,
                        CaughtStealing = battingStat.CaughtStealing,
                        Walks = battingStat.Walks,
                        Strikeouts = battingStat.Strikeouts,
                        BattingAverage = battingStat.BattingAverage ?? 0,
                        Obp = battingStat.Obp ?? 0,
                        Slg = battingStat.Slg ?? 0,
                        Ops = battingStat.Ops ?? 0,
                        OpsPlus = battingStat.OpsPlus ?? 0,
                        TotalBases = battingStat.TotalBases,
                        HitByPitch = battingStat.HitByPitch,
                        SacrificeHits = battingStat.SacrificeHits,
                        SacrificeFlies = battingStat.SacrificeFlies,
                        Errors = battingStat.Errors,
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            playerOverviewDto.PlayerPlayoffBatting.AddRange(playoffBatting);

            var seasonPitching = pitchingSeasons
                .Select(x =>
                {
                    var pitchingStat = x.PitchingStats.SingleOrDefault(y => y.IsRegularSeason);
                    if (pitchingStat is null) return null;

                    return new PlayerPitchingOverviewDto
                    {
                        SeasonNumber = x.SeasonNumber,
                        Age = x.Age,
                        TeamNames = x.TeamNames,
                        Salary = x.Salary,
                        Traits = x.Traits,
                        Games = pitchingStat.GamesPlayed,
                        GamesStarted = pitchingStat.GamesStarted,
                        Wins = pitchingStat.Wins,
                        Losses = pitchingStat.Losses,
                        Saves = pitchingStat.Saves,
                        InningsPitched = pitchingStat.InningsPitched ?? 0,
                        Strikeouts = pitchingStat.Strikeouts,
                        EarnedRuns = pitchingStat.EarnedRuns,
                        Walks = pitchingStat.Walks,
                        Hits = pitchingStat.Hits,
                        HomeRuns = pitchingStat.HomeRuns,
                        Whip = pitchingStat.Whip ?? 0,
                        Era = pitchingStat.EarnedRunAverage ?? 0,
                        EraMinus = pitchingStat.EraMinus ?? 0,
                        PitchTypes = x.PitchTypes,
                        Fip = pitchingStat.Fip ?? 0,
                        FipMinus = pitchingStat.FipMinus ?? 0,
                        HitsPerNine = pitchingStat.HitsPerNine ?? 0,
                        HomeRunsPerNine = pitchingStat.HomeRunsPerNine ?? 0,
                        WalksPerNine = pitchingStat.WalksPerNine ?? 0,
                        StrikeoutsPerNine = pitchingStat.StrikeoutsPerNine ?? 0,
                        StrikeoutToWalkRatio = pitchingStat.StrikeoutsPerWalk ?? 0,
                        GamesFinished = pitchingStat.GamesFinished,
                        CompleteGames = pitchingStat.CompleteGames,
                        Shutouts = pitchingStat.Shutouts,
                        HitByPitch = pitchingStat.HitByPitch,
                        BattersFaced = pitchingStat.BattersFaced
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            playerOverviewDto.PlayerSeasonPitching.AddRange(seasonPitching);

            var playoffPitching = pitchingSeasons
                .Select(x =>
                {
                    var pitchingStat = x.PitchingStats.SingleOrDefault(y => !y.IsRegularSeason);
                    if (pitchingStat is null) return null;

                    return new PlayerPitchingOverviewDto
                    {
                        SeasonNumber = x.SeasonNumber,
                        Age = x.Age,
                        TeamNames = x.TeamNames,
                        Salary = x.Salary,
                        Traits = x.Traits,
                        Games = pitchingStat.GamesPlayed,
                        GamesStarted = pitchingStat.GamesStarted,
                        Wins = pitchingStat.Wins,
                        Losses = pitchingStat.Losses,
                        Saves = pitchingStat.Saves,
                        InningsPitched = pitchingStat.InningsPitched ?? 0,
                        Strikeouts = pitchingStat.Strikeouts,
                        EarnedRuns = pitchingStat.EarnedRuns,
                        Walks = pitchingStat.Walks,
                        Hits = pitchingStat.Hits,
                        HomeRuns = pitchingStat.HomeRuns,
                        Whip = pitchingStat.Whip ?? 0,
                        Era = pitchingStat.EarnedRunAverage ?? 0,
                        EraMinus = pitchingStat.EraMinus ?? 0,
                        PitchTypes = x.PitchTypes,
                        Fip = pitchingStat.Fip ?? 0,
                        FipMinus = pitchingStat.FipMinus ?? 0,
                        HitsPerNine = pitchingStat.HitsPerNine ?? 0,
                        HomeRunsPerNine = pitchingStat.HomeRunsPerNine ?? 0,
                        WalksPerNine = pitchingStat.WalksPerNine ?? 0,
                        StrikeoutsPerNine = pitchingStat.StrikeoutsPerNine ?? 0,
                        StrikeoutToWalkRatio = pitchingStat.StrikeoutsPerWalk ?? 0,
                        GamesFinished = pitchingStat.GamesFinished,
                        CompleteGames = pitchingStat.CompleteGames,
                        Shutouts = pitchingStat.Shutouts,
                        HitByPitch = pitchingStat.HitByPitch,
                        BattersFaced = pitchingStat.BattersFaced
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            playerOverviewDto.PlayerPlayoffPitching.AddRange(playoffPitching);

            var gameStats = playerWithSeasons.PlayerSeasons
                .Select(x => new
                {
                    SeasonNumber = x.Season.Number,
                    TeamNames = string.Join(", ",
                        x.PlayerTeamHistory
                            .OrderBy(y => y.Order)
                            .Where(y => y.SeasonTeamHistory is not null)
                            .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                    Salary = x.PlayerTeamHistory
                        .SingleOrDefault(y => y.Order == 1) is null
                        ? 0
                        : x.Salary,
                    Traits = string.Join(", ", x.Traits.OrderBy(y => y.Id).Select(y => y.Name)),
                    SecondaryPosition = x.SecondaryPosition?.Name,
                    x.Age,
                    x.GameStats
                })
                .ToList();

            playerOverviewDto.GameStats = gameStats
                .Select(x => new PlayerGameStatOverviewDto
                {
                    SeasonNumber = x.SeasonNumber,
                    Age = x.Age,
                    TeamNames = x.TeamNames,
                    Salary = x.Salary,
                    Traits = x.Traits,
                    SecondaryPosition = x.SecondaryPosition,
                    Power = x.GameStats.Power,
                    Contact = x.GameStats.Contact,
                    Speed = x.GameStats.Speed,
                    Fielding = x.GameStats.Fielding,
                    Arm = x.GameStats.Arm,
                    Velocity = x.GameStats.Velocity,
                    Junk = x.GameStats.Junk,
                    Accuracy = x.GameStats.Accuracy,
                })
                .ToList();

            return playerOverviewDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerCareerDto>, Exception>> GetTopBattingCareers(int? pageNumber,
        string? orderBy,
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus);
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
                .Where(x => x.FranchiseId == franchiseId)
                .Select(x => new PlayerCareerDto
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
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * 20)
                .Take(20)
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
            }

            return playerCareerDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerCareerDto>, Exception>> GetTopPitchingCareers(int? pageNumber,
        string? orderBy,
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus);
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
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => x.PitcherRole != null)
                .Select(x => new PlayerCareerDto
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
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * 20)
                .Take(20)
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
            }

            return playerCareerDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> GetTopBattingSeasons(int seasonId,
        bool isPlayoffs,
        int? pageNumber,
        string? orderBy,
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        try
        {
            var playerBattingDtos = await _dbContext.PlayerSeasonBattingStats
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
                .ThenInclude(x => x.TeamNameHistory)
                .Where(x => seasonId == default || x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.IsRegularSeason == !isPlayoffs)
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
                    TotalSalary = x.PlayerSeason.Salary,
                    BatHandedness = x.PlayerSeason.Player.BatHandedness.Name,
                    ThrowHandedness = x.PlayerSeason.Player.ThrowHandedness.Name,
                    PrimaryPosition = x.PlayerSeason.Player.PrimaryPosition.Name,
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
                    WeightedOpsPlusOrEraMinus = (x.OpsPlus ?? 0) * x.AtBats / 10000,
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * 20)
                .Take(20)
                .ToListAsync(cancellationToken: cancellationToken);

            return playerBattingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> GetTopPitchingSeasons(int seasonId,
        bool isPlayoffs,
        int? pageNumber,
        string? orderBy,
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        try
        {
            var playerPitchingDtos = await _dbContext.PlayerSeasonPitchingStats
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
                .ThenInclude(x => x.TeamNameHistory)
                .Where(x => seasonId == default || x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.IsRegularSeason == !isPlayoffs)
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
                    TotalSalary = x.PlayerSeason.Salary,
                    BatHandedness = x.PlayerSeason.Player.BatHandedness.Name,
                    ThrowHandedness = x.PlayerSeason.Player.ThrowHandedness.Name,
                    PrimaryPosition = x.PlayerSeason.Player.PrimaryPosition.Name,
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
                })
                .OrderBy(orderBy)
                .Skip(((pageNumber ?? 1) - 1) * 20)
                .Take(20)
                .ToListAsync(cancellationToken: cancellationToken);

            return playerPitchingDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}