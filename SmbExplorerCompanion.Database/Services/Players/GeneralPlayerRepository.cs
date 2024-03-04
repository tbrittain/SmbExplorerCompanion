using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services.Players;

public class GeneralPlayerRepository : IGeneralPlayerRepository
{
    private static readonly string[] PositiveFieldingTraitNames = {"Cannon Arm", "Dive Wizard", "Utility", "Magic Hands"};
    private static readonly string[] NegativeFieldingTraitNames = {"Butter Fingers", "Noodle Arm", "Wild Thrower"};
    private readonly IApplicationContext _applicationContext;
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IPitcherCareerRepository _pitcherCareerRepository;
    private readonly IPitcherSeasonRepository _pitcherSeasonRepository;
    private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;
    private readonly IPositionPlayerSeasonRepository _positionPlayerSeasonRepository;

    public GeneralPlayerRepository(SmbExplorerCompanionDbContext dbContext,
        IPositionPlayerCareerRepository positionPlayerCareerRepository,
        IPitcherCareerRepository pitcherCareerRepository,
        IPositionPlayerSeasonRepository positionPlayerSeasonRepository,
        IPitcherSeasonRepository pitcherSeasonRepository,
        IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _positionPlayerCareerRepository = positionPlayerCareerRepository;
        _pitcherCareerRepository = pitcherCareerRepository;
        _positionPlayerSeasonRepository = positionPlayerSeasonRepository;
        _pitcherSeasonRepository = pitcherSeasonRepository;
        _applicationContext = applicationContext;
    }

    public async Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var playerCareerBattingResult =
                await _positionPlayerCareerRepository.GetBattingCareers(playerId: playerId, cancellationToken: cancellationToken);

            if (playerCareerBattingResult.TryPickT1(out var exception, out var playerCareerBattingDtos))
                return exception;

            var playerCareerPitchingResult =
                await _pitcherCareerRepository.GetPitchingCareers(playerId: playerId, cancellationToken: cancellationToken);

            if (playerCareerPitchingResult.TryPickT1(out exception, out var playerCareerPitchingDtos))
                return exception;

            var playerOverview = await GetPlayerOverview(playerId, cancellationToken);

            if (playerCareerBattingDtos.Any()) playerOverview.CareerBatting = playerCareerBattingDtos.First();

            if (playerCareerPitchingDtos.Any()) playerOverview.CareerPitching = playerCareerPitchingDtos.First();

            var playerSeasonBatting =
                await _positionPlayerSeasonRepository.GetBattingSeasons(playerId: playerId, cancellationToken: cancellationToken);

            if (playerSeasonBatting.TryPickT1(out exception, out var playerSeasonBattingDtos))
                return exception;

            playerOverview.PlayerSeasonBatting = playerSeasonBattingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var playerSeasonPitching = await _pitcherSeasonRepository.GetPitchingSeasons(playerId: playerId, cancellationToken: cancellationToken);

            if (playerSeasonPitching.TryPickT1(out exception, out var playerSeasonPitchingDtos))
                return exception;

            playerOverview.PlayerSeasonPitching = playerSeasonPitchingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var playerPlayoffBatting =
                await _positionPlayerSeasonRepository.GetBattingSeasons(playerId: playerId,
                    isPlayoffs: true,
                    cancellationToken: cancellationToken);

            if (playerPlayoffBatting.TryPickT1(out exception, out var playerPlayoffBattingDtos))
                return exception;

            playerOverview.PlayerPlayoffBatting = playerPlayoffBattingDtos
                .OrderByDescending(x => x.SeasonNumber)
                .ToList();

            var playerPlayoffPitching =
                await _pitcherSeasonRepository.GetPitchingSeasons(playerId: playerId, isPlayoffs: true, cancellationToken: cancellationToken);

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
                    SeasonId = x.PlayerSeason.SeasonId,
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

    public async Task<OneOf<GameStatDto, Exception>> GetLeagueAverageGameStats(int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = _dbContext.PlayerSeasonGameStats
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .Where(x => x.PlayerSeason.SeasonId == seasonId);

            if (isPitcher)
            {
                queryable = pitcherRoleId.HasValue
                    ? queryable.Where(x => x.PlayerSeason.Player.PitcherRoleId == pitcherRoleId)
                    : queryable.Where(x => x.PlayerSeason.Player.PitcherRoleId != null);
            }

            var averageGameStats = await queryable
                .GroupBy(x => 1)
                .Select(x => new GameStatDto
                {
                    Power = (int) Math.Round(x.Average(y => y.Power)),
                    Contact = (int) Math.Round(x.Average(y => y.Contact)),
                    Speed = (int) Math.Round(x.Average(y => y.Speed)),
                    Fielding = (int) Math.Round(x.Average(y => y.Fielding)),
                    Arm = (int) Math.Round(x.Average(y => y.Arm ?? 0)),
                    Velocity = (int) Math.Round(x.Average(y => y.Velocity ?? 0)),
                    Junk = (int) Math.Round(x.Average(y => y.Junk ?? 0)),
                    Accuracy = (int) Math.Round(x.Average(y => y.Accuracy ?? 0))
                })
                .SingleAsync(cancellationToken: cancellationToken);

            return averageGameStats;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<PlayerGameStatPercentileDto, Exception>> GetPlayerGameStatPercentiles(int playerId,
        int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var playerGameStats = await _dbContext.PlayerSeasonGameStats
                .Include(x => x.PlayerSeason)
                .Where(x => x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.PlayerSeason.PlayerId == playerId)
                .FirstAsync(cancellationToken: cancellationToken);

            var power = playerGameStats.Power;
            var contact = playerGameStats.Contact;
            var speed = playerGameStats.Speed;
            var fielding = playerGameStats.Fielding;
            var arm = playerGameStats.Arm;
            var velocity = playerGameStats.Velocity;
            var junk = playerGameStats.Junk;
            var accuracy = playerGameStats.Accuracy;

            var queryable = _dbContext.PlayerSeasonGameStats
                .Include(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .Where(x => x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.PlayerSeason.PlayerId != playerId);

            if (isPitcher)
            {
                queryable = pitcherRoleId.HasValue
                    ? queryable.Where(x => x.PlayerSeason.Player.PitcherRoleId == pitcherRoleId)
                    : queryable.Where(x => x.PlayerSeason.Player.PitcherRoleId != null);
            }

            var numPlayers = await queryable
                .Select(x => x.PlayerSeason.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken: cancellationToken);

            var greaterThanPower = await queryable
                .Where(x => power > x.Power)
                .Select(x => x.PlayerSeason.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken: cancellationToken);

            var greaterThanContact = await queryable
                .Where(x => contact > x.Contact)
                .Select(x => x.PlayerSeason.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken: cancellationToken);

            var greaterThanSpeed = await queryable
                .Where(x => speed > x.Speed)
                .Select(x => x.PlayerSeason.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken: cancellationToken);

            var greaterThanFielding = await queryable
                .Where(x => fielding > x.Fielding)
                .Select(x => x.PlayerSeason.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken: cancellationToken);

            var greaterThanVelocity = 0;
            var greaterThanJunk = 0;
            var greaterThanAccuracy = 0;
            var greaterThanArm = 0;

            if (!isPitcher)
            {
                greaterThanArm = await queryable
                    .Where(x => arm > x.Arm)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);
            }
            else
            {
                greaterThanVelocity = await queryable
                    .Where(x => velocity > x.Velocity)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                greaterThanJunk = await queryable
                    .Where(x => junk > x.Junk)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                greaterThanAccuracy = await queryable
                    .Where(x => accuracy > x.Accuracy)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);
            }

            var playerGameStatPercentileDto = new PlayerGameStatPercentileDto
            {
                Power = RoundPercentile(greaterThanPower, numPlayers),
                Contact = RoundPercentile(greaterThanContact, numPlayers),
                Speed = RoundPercentile(greaterThanSpeed, numPlayers),
                Fielding = RoundPercentile(greaterThanFielding, numPlayers),
                Arm = RoundPercentile(greaterThanArm, numPlayers),
                Velocity = RoundPercentile(greaterThanVelocity, numPlayers),
                Junk = RoundPercentile(greaterThanJunk, numPlayers),
                Accuracy = RoundPercentile(greaterThanAccuracy, numPlayers)
            };
            return playerGameStatPercentileDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private static int RoundPercentile(int numGreaterThan, int numPlayers)
    {
        return (int) Math.Round(numGreaterThan / (double) numPlayers * 100);
    }

    public async Task<OneOf<PlayerKpiPercentileDto, Exception>> GetPlayerKpiPercentiles(
        int playerId,
        int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var season = await _dbContext.Seasons
                .Where(x => x.Id == seasonId)
                .SingleAsync(cancellationToken: cancellationToken);

            var playerKpiPercentileDto = new PlayerKpiPercentileDto();

            var playerBattingStats = await _dbContext.PlayerSeasonBattingStats
                .Include(x => x.PlayerSeason)
                .Where(x => x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.PlayerSeason.PlayerId == playerId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (playerBattingStats is not null && playerBattingStats.PlateAppearances > 0)
            {
                var plateAppearances = playerBattingStats.PlateAppearances;
                var hits = playerBattingStats.Hits;
                var homeRuns = playerBattingStats.HomeRuns;
                var battingAverage = playerBattingStats.BattingAverage ?? 0;
                var stolenBases = playerBattingStats.StolenBases;
                var batterStrikeouts = playerBattingStats.Strikeouts;
                var obp = playerBattingStats.Obp ?? 0;
                var slg = playerBattingStats.Slg ?? 0;

                var battingQueryable = _dbContext.PlayerSeasonBattingStats
                    .Include(x => x.PlayerSeason)
                    .ThenInclude(x => x.Player)
                    .Where(x => x.PlayerSeason.SeasonId == seasonId)
                    .Where(x => x.PlayerSeason.PlayerId != playerId);

                var numQualifiedPlayers = await battingQueryable
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanHits = await battingQueryable
                    .Where(x => (hits / (double) plateAppearances) > (x.Hits / (double) x.PlateAppearances))
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanHomeRuns = await battingQueryable
                    .Where(x => (homeRuns / (double) plateAppearances) > (x.HomeRuns / (double) x.PlateAppearances))
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanBattingAverage = await battingQueryable
                    .Where(x => battingAverage > x.BattingAverage)
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanStolenBases = await battingQueryable
                    .Where(x => (stolenBases / (double) plateAppearances) > (x.StolenBases / (double) x.PlateAppearances))
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var lessThanBatterStrikeouts = await battingQueryable
                    .Where(x => (batterStrikeouts / (double) plateAppearances) < (x.Strikeouts / (double) x.PlateAppearances))
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanObp = await battingQueryable
                    .Where(x => obp > x.Obp)
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanSlg = await battingQueryable
                    .Where(x => slg > x.Slg)
                    .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);
    
                playerKpiPercentileDto.Hits = RoundPercentile(greaterThanHits, numQualifiedPlayers);
                playerKpiPercentileDto.HomeRuns = RoundPercentile(greaterThanHomeRuns, numQualifiedPlayers);
                playerKpiPercentileDto.BattingAverage = RoundPercentile(greaterThanBattingAverage, numQualifiedPlayers);
                playerKpiPercentileDto.StolenBases = RoundPercentile(greaterThanStolenBases, numQualifiedPlayers);
                playerKpiPercentileDto.BatterStrikeouts = RoundPercentile(lessThanBatterStrikeouts, numQualifiedPlayers);
                playerKpiPercentileDto.Obp = RoundPercentile(greaterThanObp, numQualifiedPlayers);
                playerKpiPercentileDto.Slg = RoundPercentile(greaterThanSlg, numQualifiedPlayers);
            }

            if (!isPitcher) return playerKpiPercentileDto;

            var playerPitchingStats = await _dbContext.PlayerSeasonPitchingStats
                .Include(x => x.PlayerSeason)
                .Where(x => x.PlayerSeason.SeasonId == seasonId)
                .Where(x => x.PlayerSeason.PlayerId == playerId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (playerPitchingStats is not null && playerPitchingStats.InningsPitched > 0)
            {
                var wins = playerPitchingStats.Wins;
                var era = playerPitchingStats.EarnedRunAverage ?? 0;
                var whip = playerPitchingStats.Whip ?? 0;
                var inningsPitched = playerPitchingStats.InningsPitched ?? 0;
                var pitcherStrikeoutsPerNine = playerPitchingStats.StrikeoutsPerNine ?? 0;
                var pitcherStrikeoutToWalkRatio = playerPitchingStats.StrikeoutsPerWalk ?? 0;

                var pitchingQueryable = _dbContext.PlayerSeasonPitchingStats
                    .Include(x => x.PlayerSeason)
                    .ThenInclude(x => x.Player)
                    .Where(x => x.PlayerSeason.SeasonId == seasonId)
                    .Where(x => x.PlayerSeason.PlayerId != playerId);

                if (pitcherRoleId.HasValue)
                {
                    pitchingQueryable = pitchingQueryable
                        .Where(x => x.PlayerSeason.Player.PitcherRoleId == pitcherRoleId);
                }

                var numPlayers = await pitchingQueryable
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanWins = await pitchingQueryable
                    .Where(x => wins >= x.Wins)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var lessThanEra = await pitchingQueryable
                    .Where(x => era <= x.EarnedRunAverage)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var lessThanWhip = await pitchingQueryable
                    .Where(x => whip <= x.Whip)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanInningsPitched = await pitchingQueryable
                    .Where(x => inningsPitched >= x.InningsPitched)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanPitcherStrikeoutsPerNine = await pitchingQueryable
                    .Where(x => pitcherStrikeoutsPerNine >= x.StrikeoutsPerNine)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                var greaterThanPitcherStrikeoutToWalkRatio = await pitchingQueryable
                    .Where(x => pitcherStrikeoutToWalkRatio >= x.StrikeoutsPerWalk)
                    .Select(x => x.PlayerSeason.PlayerId)
                    .Distinct()
                    .CountAsync(cancellationToken: cancellationToken);

                playerKpiPercentileDto.Wins = RoundPercentile(greaterThanWins, numPlayers);
                playerKpiPercentileDto.Era = RoundPercentile(lessThanEra, numPlayers);
                playerKpiPercentileDto.Whip = RoundPercentile(lessThanWhip, numPlayers);
                playerKpiPercentileDto.InningsPitched = RoundPercentile(greaterThanInningsPitched, numPlayers);
                playerKpiPercentileDto.PitcherStrikeoutsPerNine = RoundPercentile(greaterThanPitcherStrikeoutsPerNine, numPlayers);
                playerKpiPercentileDto.PitcherStrikeoutToWalkRatio = RoundPercentile(greaterThanPitcherStrikeoutToWalkRatio, numPlayers);
            }

            return playerKpiPercentileDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<PlayerBaseDto, Exception>> GetRandomPlayer()
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;
        var queryable = _dbContext.Players
            .Where(x => x.FranchiseId == franchiseId);

        return await queryable
            .OrderBy(x => EF.Functions.Random())
            .Select(x => new PlayerBaseDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}"
            })
            .FirstAsync();
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
        playerOverview.IsPitcher = player.PitcherRoleId is not null;
        playerOverview.TotalSalary = player.PlayerSeasons
            .Sum(x => x.PlayerTeamHistory
                .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                ? 0
                : x.Salary);
        playerOverview.BatHandednessId = player.BatHandednessId;
        playerOverview.ThrowHandednessId = player.ThrowHandednessId;
        playerOverview.PrimaryPositionId = player.PrimaryPositionId;
        playerOverview.PitcherRoleId = player.PitcherRoleId;
        playerOverview.ChemistryId = player.ChemistryId;
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
            .Sum(y => ((y.OpsPlus ?? 0) - 95) * y.PlateAppearances * BattingScalingFactor +
                      (y.StolenBases - y.CaughtStealing) * BaserunningScalingFactor);

        var weightedEraMinus = player.PlayerSeasons
            .SelectMany(y => y.PitchingStats)
            .Where(y => y is {EraMinus: not null, InningsPitched: not null})
            .Sum(y => (((y.EraMinus ?? 0) + (y.FipMinus ?? 0)) / 2 - 95) * (y.InningsPitched ?? 0) * PitchingScalingFactor);

        var weightedOpsPlusOrEraMinus = weightedOpsPlus + weightedEraMinus;
        playerOverview.WeightedOpsPlusOrEraMinus = weightedOpsPlusOrEraMinus;

        return playerOverview;
    }
}