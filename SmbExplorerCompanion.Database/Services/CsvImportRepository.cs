using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Database.Entities;

namespace SmbExplorerCompanion.Database.Services;

public class CsvImportRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public CsvImportRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<Success, Exception>> AddTeamsAsync(List<CsvTeam> teams, int franchiseId)
    {
        try
        {
            foreach (var csvTeam in teams)
            {
                var season = await _dbContext.Seasons
                    .SingleOrDefaultAsync(x => x.Id == csvTeam.SeasonId);
                if (season is null)
                {
                    season = new Season
                    {
                        Id = csvTeam.SeasonId,
                        Number = csvTeam.SeasonNum
                    };
                    _dbContext.Seasons.Add(season);
                    await _dbContext.SaveChangesAsync();
                }

                var conference = await _dbContext.Conferences
                    .SingleOrDefaultAsync(x => x.Name == csvTeam.ConferenceName);
                if (conference is null)
                {
                    conference = new Conference
                    {
                        FranchiseId = franchiseId,
                        Name = csvTeam.ConferenceName
                    };
                    _dbContext.Conferences.Add(conference);
                    await _dbContext.SaveChangesAsync();
                }

                var division = await _dbContext.Divisions
                    .SingleOrDefaultAsync(x => x.Name == csvTeam.DivisionName);
                if (division is null)
                {
                    division = new Division
                    {
                        ConferenceId = conference.Id,
                        Name = csvTeam.DivisionName
                    };
                    _dbContext.Divisions.Add(division);
                    await _dbContext.SaveChangesAsync();
                }

                Team? team;

                var teamGameIdHistory = await _dbContext.TeamGameIdHistory
                    .Include(x => x.Team)
                    .ThenInclude(x => x.SeasonTeamHistory)
                    .ThenInclude(x => x.TeamNameHistory)
                    .SingleOrDefaultAsync(x => x.GameId == csvTeam.TeamId);
                if (teamGameIdHistory is null)
                {
                    // attempt to locate team based on the team name
                    var teamNameHistory = await _dbContext.TeamNameHistory
                        .Include(x => x.SeasonTeamHistory)
                        .ThenInclude(x => x.Team)
                        .SingleOrDefaultAsync(x => x.Name == csvTeam.TeamName);

                    // assume the team is new and does not exist if there is no equivalent team matching the name
                    if (teamNameHistory is null)
                    {
                        team = new Team
                        {
                            TeamGameIdHistory = new List<TeamGameIdHistory>
                            {
                                new()
                                {
                                    GameId = csvTeam.TeamId
                                }
                            }
                        };
                        _dbContext.Teams.Add(team);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        team = teamNameHistory.SeasonTeamHistory.First().Team;
                    }
                }
                else
                {
                    team = teamGameIdHistory.Team;
                }

                var newSeasonTeamHistory = false;
                var seasonTeamHistory = await _dbContext.SeasonTeamHistory
                    .Include(x => x.Team)
                    .SingleOrDefaultAsync(x => x.TeamId == team.Id && x.SeasonId == season.Id);

                if (seasonTeamHistory is null)
                {
                    newSeasonTeamHistory = true;
                    seasonTeamHistory = new SeasonTeamHistory
                    {
                        SeasonId = season.Id,
                        TeamId = team.Id
                    };
                }

                seasonTeamHistory.Budget = csvTeam.Budget;
                seasonTeamHistory.Payroll = csvTeam.Payroll;
                seasonTeamHistory.Surplus = csvTeam.Surplus;
                seasonTeamHistory.SurplusPerGame = csvTeam.SurplusPerGame;
                seasonTeamHistory.Wins = csvTeam.Wins;
                seasonTeamHistory.Losses = csvTeam.Losses;
                seasonTeamHistory.GamesBehind = csvTeam.GamesBehind;
                seasonTeamHistory.WinPercentage = csvTeam.WinPercentage;
                seasonTeamHistory.PythagoreanWinPercentage = csvTeam.PythagoreanWinPercentage;
                seasonTeamHistory.ExpectedWins = csvTeam.ExpectedWins;
                seasonTeamHistory.ExpectedLosses = csvTeam.ExpectedLosses;
                seasonTeamHistory.RunsScored = csvTeam.RunsFor;
                seasonTeamHistory.RunsAllowed = csvTeam.RunsAgainst;
                seasonTeamHistory.TotalPower = csvTeam.TotalPower;
                seasonTeamHistory.TotalContact = csvTeam.TotalContact;
                seasonTeamHistory.TotalSpeed = csvTeam.TotalSpeed;
                seasonTeamHistory.TotalFielding = csvTeam.TotalFielding;
                seasonTeamHistory.TotalArm = csvTeam.TotalArm;
                seasonTeamHistory.TotalVelocity = csvTeam.TotalVelocity;
                seasonTeamHistory.TotalJunk = csvTeam.TotalJunk;
                seasonTeamHistory.TotalAccuracy = csvTeam.TotalAccuracy;
                seasonTeamHistory.DivisionId = division.Id;

                // get the last team name history for this team, and if the name is different, add a new team name history
                var lastTeamNameHistory = team.SeasonTeamHistory.Last().TeamNameHistory;
                if (lastTeamNameHistory.Name != csvTeam.TeamName)
                {
                    var teamNameHistory = new TeamNameHistory
                    {
                        Name = csvTeam.TeamName
                    };
                    _dbContext.TeamNameHistory.Add(teamNameHistory);
                    await _dbContext.SaveChangesAsync();

                    seasonTeamHistory.TeamNameHistoryId = teamNameHistory.Id;
                }
                else
                {
                    seasonTeamHistory.TeamNameHistoryId = lastTeamNameHistory.Id;
                }

                if (newSeasonTeamHistory)
                    _dbContext.SeasonTeamHistory.Add(seasonTeamHistory);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return new Success();
    }
}