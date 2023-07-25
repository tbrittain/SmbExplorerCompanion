using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

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
                .ThenInclude(x => x.PitchingStats)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.HomePlayoffSchedule)
                .Include(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.AwayPlayoffSchedule)
                .ToListAsync(cancellationToken: cancellationToken);

            var maxPlayoffSeries = await _dbContext.TeamPlayoffSchedules
                .MaxAsync(y => y.SeriesNumber, cancellationToken: cancellationToken);

            var historicalTeams = teams
                .Select(x =>
                {
                    var team = new HistoricalTeamDto
                    {
                        Id = x.Id
                    };

                    team.CurrentName = x.SeasonTeamHistory
                        .OrderByDescending(y => y.SeasonId)
                        .First().TeamNameHistory.Name;

                    team.NumGames = x.SeasonTeamHistory.Sum(y => y.Wins + y.Losses);
                    team.NumWins = x.SeasonTeamHistory.Sum(y => y.Wins);
                    team.NumLosses = x.SeasonTeamHistory.Sum(y => y.Losses);
                    team.NumDivisionsWon = x.SeasonTeamHistory.Count(y => y.GamesBehind == 0);
                    team.NumChampionships = x.SeasonTeamHistory
                        .Count(y => y.ChampionshipWinner is not null);
                    team.NumPlayoffAppearances = x.SeasonTeamHistory
                        .Count(y => y.HomePlayoffSchedule.Any() || y.AwayPlayoffSchedule.Any());

                    team.NumRunsScored = x.SeasonTeamHistory
                        .Sum(y => y.RunsScored + y.PlayoffRunsScored.GetValueOrDefault());

                    team.NumRunsAllowed = x.SeasonTeamHistory
                        .Sum(y => y.RunsAllowed + y.PlayoffRunsAllowed.GetValueOrDefault());

                    var teamPlayers = x.SeasonTeamHistory
                        .SelectMany(y => y.PlayerTeamHistory)
                        .ToList();

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

                    team.BattingAverage = teamPlayers
                        .SelectMany(y => y.PlayerSeason.BattingStats)
                        .Where(y => y.BattingAverage is not null)
                        .Average(y => y.BattingAverage!.Value);

                    team.EarnedRunAverage = teamPlayers
                        .SelectMany(y => y.PlayerSeason.PitchingStats)
                        .Where(y => y.EarnedRunAverage is not null)
                        .Average(y => y.EarnedRunAverage!.Value);

                    var numConferenceTitles = x.SeasonTeamHistory
                        .Select(seasonTeamHistory => seasonTeamHistory.HomePlayoffSchedule
                            .Where(y => y.SeriesNumber == maxPlayoffSeries)
                            .ToList())
                        .Count(homePlayoffSchedule => homePlayoffSchedule.Any());

                    team.NumConferenceTitles = numConferenceTitles;

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
}