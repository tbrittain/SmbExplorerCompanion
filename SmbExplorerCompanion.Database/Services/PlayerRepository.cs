using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class PlayerRepository : IPlayerRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PlayerRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId, CancellationToken cancellationToken)
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
            playerOverviewDto.IsPitcher = playerWithSeasons.PitcherRole is not null;
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
                                .Sum(x => x.BattingStats.Sum(y => y.PlateAppearances)) +
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
                   (playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Doubles)) * 2) +
                   (playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.Triples)) * 3) +
                   (playerWithSeasons.PlayerSeasons
                       .Sum(x => x.BattingStats.Sum(y => y.HomeRuns)) * 4)) /
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
                .Sum(x => x.PitchingStats.Sum(y => y.EarnedRuns)) == 0
                ? 0
                : playerWithSeasons.PlayerSeasons
                      .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched ?? 0)) /
                  playerWithSeasons.PlayerSeasons
                      .Sum(x => x.PitchingStats.Sum(y => y.EarnedRuns));
            playerOverviewDto.Games = playerWithSeasons.PlayerSeasons
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
                        PitchTypes = x.PitchTypes
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
                        PitchTypes = x.PitchTypes
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
}