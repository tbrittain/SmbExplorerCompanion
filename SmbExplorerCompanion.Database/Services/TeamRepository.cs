using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Entities;

namespace SmbExplorerCompanion.Database.Services;

public class TeamRepository : ITeamRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IApplicationContext _applicationContext;

    public TeamRepository(SmbExplorerCompanionDbContext dbContext, IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    public async Task<OneOf<IEnumerable<HistoricalTeamDto>, Exception>> GetHistoricalTeams(CancellationToken cancellationToken)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;
        var teamsQueryable = _dbContext
            .Teams
            .Include(x => x.SeasonTeamHistory)
            .ThenInclude(x => x.Division)
            .ThenInclude(x => x.Conference)
            .Where(x => x.SeasonTeamHistory.First().Division.Conference.FranchiseId == franchiseId);

        try
        {
            var maxPlayoffSeries = await _dbContext.TeamPlayoffSchedules
                .MaxAsync(y => y.SeriesNumber, cancellationToken: cancellationToken);

            var teams = await teamsQueryable
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.TeamNameHistory)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.PlayerSeason)
                .ThenInclude(x => x.BattingStats)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.PlayerSeason)
                .ThenInclude(x => x.PitchingStats)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.HomePlayoffSchedule)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.AwayPlayoffSchedule)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.ChampionshipWinner)
                .Select(x => new
                {
                    x.Id,
                    CurrentName = x.SeasonTeamHistory
                        .OrderByDescending(y => y.SeasonId)
                        .First().TeamNameHistory.Name,
                    NumGames = x.SeasonTeamHistory.Sum(y => y.Wins + y.Losses),
                    NumWins = x.SeasonTeamHistory.Sum(y => y.Wins),
                    NumLosses = x.SeasonTeamHistory.Sum(y => y.Losses),
                    NumDivisionsWon = x.SeasonTeamHistory.Count(y => y.GamesBehind == 0),
                    NumChampionships = x.SeasonTeamHistory
                        .Count(y => y.ChampionshipWinner != null),
                    NumPlayoffAppearances = x.SeasonTeamHistory
                        .Count(y => y.HomePlayoffSchedule.Any() || y.AwayPlayoffSchedule.Any()),
                    NumRunsScored = x.SeasonTeamHistory
                        .Sum(y => y.RunsScored + (y.PlayoffRunsScored ?? 0)),
                    NumRunsAllowed = x.SeasonTeamHistory
                        .Sum(y => y.RunsAllowed + (y.PlayoffRunsAllowed ?? 0)),
                    NumConferenceTitles = x.SeasonTeamHistory
                        .Select(seasonTeamHistory => seasonTeamHistory.HomePlayoffSchedule
                            .Where(y => y.SeriesNumber == maxPlayoffSeries)
                            .ToList())
                        .Count(homePlayoffSchedule => homePlayoffSchedule.Any())
                })
                .ToListAsync(cancellationToken: cancellationToken);

            Dictionary<int, List<PlayerTeamHistory>> playerTeamHistories = new();
            foreach (var team in teams)
            {
                // TODO: May want to do the same aggregations here as we did above, but for team-player-specific stats
                var histories = await _dbContext.SeasonTeamHistory
                    .Include(x => x.PlayerTeamHistory)
                    .ThenInclude(x => x.PlayerSeason)
                    .ThenInclude(x => x.Player)
                    .Include(x => x.PlayerTeamHistory)
                    .ThenInclude(x => x.PlayerSeason)
                    .ThenInclude(x => x.BattingStats)
                    .Include(x => x.PlayerTeamHistory)
                    .ThenInclude(x => x.PlayerSeason)
                    .ThenInclude(x => x.PitchingStats)
                    .Where(x => x.TeamId == team.Id)
                    .SelectMany(y => y.PlayerTeamHistory)
                    .ToListAsync(cancellationToken: cancellationToken);

                playerTeamHistories.Add(team.Id, histories);
            }

            var historicalTeams = teams
                .Select(x =>
                {
                    var team = new HistoricalTeamDto
                    {
                        TeamId = x.Id,
                        CurrentName = x.CurrentName,
                        NumGames = x.NumGames,
                        NumWins = x.NumWins,
                        NumLosses = x.NumLosses,
                        NumDivisionsWon = x.NumDivisionsWon,
                        NumChampionships = x.NumChampionships,
                        NumConferenceTitles = x.NumConferenceTitles,
                        NumPlayoffAppearances = x.NumPlayoffAppearances,
                        NumRunsScored = x.NumRunsScored,
                        NumRunsAllowed = x.NumRunsAllowed,
                    };

                    var teamPlayers = playerTeamHistories[x.Id];

                    team.NumPlayers = teamPlayers
                        .Select(y => y.PlayerSeason.PlayerId)
                        .Distinct()
                        .Count();

                    team.NumHallOfFamers = teamPlayers
                        .Select(y => y.PlayerSeason.Player)
                        .DistinctBy(y => y.Id)
                        .Count(y => y.IsHallOfFamer);

                    team.NumAtBats = teamPlayers
                        .SelectMany(y => y.PlayerSeason.BattingStats)
                        .Sum(y => y.AtBats);

                    team.NumHits = teamPlayers
                        .SelectMany(y => y.PlayerSeason.BattingStats)
                        .Sum(y => y.Hits);

                    team.NumHomeRuns = teamPlayers
                        .SelectMany(y => y.PlayerSeason.BattingStats)
                        .Sum(y => y.HomeRuns);

                    var hits = teamPlayers
                        .SelectMany(y => y.PlayerSeason.BattingStats)
                        .Sum(y => y.Hits);

                    team.BattingAverage = (double) hits / team.NumAtBats;

                    var earnedRuns = teamPlayers
                        .SelectMany(y => y.PlayerSeason.PitchingStats)
                        .Sum(y => y.EarnedRuns);

                    var inningsPitched = teamPlayers
                        .SelectMany(y => y.PlayerSeason.PitchingStats)
                        .Sum(y => y.InningsPitched ?? 0);

                    team.EarnedRunAverage = earnedRuns / inningsPitched * 9;

                    return team;
                })
                .OrderBy(x => x.CurrentName)
                .ToList();

            if (cancellationToken.IsCancellationRequested)
                return Array.Empty<HistoricalTeamDto>();

            return historicalTeams;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<TeamOverviewDto, Exception>> GetTeamOverview(int teamId, CancellationToken cancellationToken)
    {
        try
        {
            var maxPlayoffSeries = await _dbContext.TeamPlayoffSchedules
                .MaxAsync(y => y.SeriesNumber, cancellationToken: cancellationToken);

            var teamOverviewDto = await _dbContext.SeasonTeamHistory
                .Include(x => x.TeamNameHistory)
                .Include(x => x.ChampionshipWinner)
                .Include(x => x.HomePlayoffSchedule)
                .Include(x => x.AwayPlayoffSchedule)
                .Where(x => x.TeamId == teamId)
                .GroupBy(_ => true)
                .Select(x => new TeamOverviewDto
                {
                    TeamId = teamId,
                    CurrentTeamName = x.OrderByDescending(y => y.SeasonId).First().TeamNameHistory.Name,
                    NumSeasons = x.Count(),
                    NumWins = x.Sum(y => y.Wins),
                    NumLosses = x.Sum(y => y.Losses),
                    NumPlayoffAppearances = x.Count(y => y.HomePlayoffSchedule.Any() || y.AwayPlayoffSchedule.Any()),
                    NumDivisionsWon = x.Count(y => y.GamesBehind == 0),
                    NumConferenceTitles = x
                        .Select(seasonTeamHistory => seasonTeamHistory.HomePlayoffSchedule
                            .Where(y => y.SeriesNumber == maxPlayoffSeries)
                            .ToList())
                        .Count(homePlayoffSchedule => homePlayoffSchedule.Any()),
                    NumChampionships = x.Count(y => y.ChampionshipWinner != null),
                })
                .FirstAsync(cancellationToken: cancellationToken);

            var teamHistories = await _dbContext.SeasonTeamHistory
                .Include(x => x.Season)
                .Include(x => x.Division)
                .ThenInclude(x => x.Conference)
                .Include(x => x.TeamNameHistory)
                .Include(x => x.ChampionshipWinner)
                .Include(x => x.HomePlayoffSchedule)
                .Include(x => x.AwayPlayoffSchedule)
                .Where(x => x.TeamId == teamId)
                .ToListAsync(cancellationToken: cancellationToken);

            teamOverviewDto.TeamHistory = teamHistories
                .Select(x => new TeamOverviewSeasonHistoryDto
                {
                    SeasonTeamId = x.Id,
                    SeasonNumber = x.Season.Number,
                    DivisionName = x.Division.Name,
                    ConferenceName = x.Division.Conference.Name,
                    TeamName = x.TeamNameHistory.Name,
                    Wins = x.Wins,
                    Losses = x.Losses,
                    GamesBehind = x.GamesBehind,
                    PlayoffWins = x.PlayoffWins,
                    PlayoffLosses = x.PlayoffLosses,
                    PlayoffSeed = x.PlayoffSeed,
                    WonConference = x.HomePlayoffSchedule.Any(y => y.SeriesNumber == maxPlayoffSeries) ||
                                    x.AwayPlayoffSchedule.Any(y => y.SeriesNumber == maxPlayoffSeries),
                    WonChampionship = x.ChampionshipWinner != null,
                })
                .OrderBy(x => x.SeasonNumber)
                .ToList();

            var topPlayers = await _dbContext.Players
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PlayerTeamHistory)
                .Where(x => x.PlayerSeasons
                    .Any(y => y.PlayerTeamHistory
                        .Any(z => z.SeasonTeamHistory!.TeamId == teamId)))
                .ToListAsync(cancellationToken: cancellationToken);

            teamOverviewDto.TopPlayers = topPlayers
                .Select(x => new TeamTopPlayerHistoryDto
                {
                    PlayerId = x.Id,
                    NumSeasonsWithTeam = x.PlayerSeasons
                        .Count(y => y.PlayerTeamHistory
                            .Any(z => z.SeasonTeamHistory!.TeamId == teamId)),
                    AverageOpsPlus = x.PlayerSeasons
                        .Where(y => y.PlayerTeamHistory
                            .Any(z => z.SeasonTeamHistory!.TeamId == teamId))
                        .SelectMany(y => y.BattingStats)
                        .Average(y => y.OpsPlus ?? 0),
                    WeightedOpsPlus = x.PlayerSeasons
                        .Where(y => y.PlayerTeamHistory
                            .Any(z => z.SeasonTeamHistory!.TeamId == teamId))
                        .SelectMany(y => y.BattingStats)
                        .Sum(y => (y.OpsPlus ?? 0) * y.AtBats),
                })
                .OrderByDescending(x => x.WeightedOpsPlus)
                .ToList();

            return teamOverviewDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}