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

    // Import step #1
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

    // Import step #2
    public async Task<OneOf<Success, Exception>> AddOverallPlayersAsync(List<CsvOverallPlayer> players)
    {
        var batHandedness = await _dbContext.BatHandedness.ToListAsync();
        var throwHandedness = await _dbContext.ThrowHandedness.ToListAsync();
        var positions = await _dbContext.Positions.ToListAsync();
        var chemistry = await _dbContext.Chemistry.ToListAsync();
        var pitcherRoles = await _dbContext.PitcherRoles.ToListAsync();
        var traits = await _dbContext.Traits.ToListAsync();
        var pitchTypes = await _dbContext.PitchTypes.ToListAsync();

        try
        {
            foreach (var csvOverallPlayer in players)
            {
                Player? player;
                var playerGameIdHistory = await _dbContext.PlayerGameIdHistory
                    .Include(x => x.Player)
                    .ThenInclude(x => x.Seasons)
                    .ThenInclude(x => x.GameStats)
                    .Include(x => x.Player)
                    .ThenInclude(x => x.Seasons)
                    .ThenInclude(x => x.SecondaryPosition)
                    .Include(x => x.Player)
                    .ThenInclude(x => x.Seasons)
                    .ThenInclude(x => x.Traits)
                    .SingleOrDefaultAsync(x => x.GameId == csvOverallPlayer.PlayerId);

                if (playerGameIdHistory is null)
                {
                    // attempt to match on player name AND position AND pitcher role (if applicable) AND chemistry AND handedness
                    // should be enough to uniquely identify a player
                    player = await _dbContext.Players
                        .Include(x => x.Chemistry)
                        .Include(x => x.BatHandedness)
                        .Include(x => x.ThrowHandedness)
                        .Include(x => x.PrimaryPosition)
                        .Include(x => x.PitcherRole)
                        .Where(x => x.Chemistry!.Name == csvOverallPlayer.Chemistry
                                    && x.BatHandedness.Name == csvOverallPlayer.BatHand
                                    && x.ThrowHandedness.Name == csvOverallPlayer.ThrowHand
                                    && x.PrimaryPosition.Name == csvOverallPlayer.Position
                                    && (x.PitcherRole == null || x.PitcherRole.Name == csvOverallPlayer.PitcherRole))
                        .SingleOrDefaultAsync();

                    if (player is null)
                    {
                        // assume it is a new player if we cannot find a match
                        player = new Player
                        {
                            FirstName = csvOverallPlayer.FirstName,
                            LastName = csvOverallPlayer.LastName,
                            Chemistry = chemistry.SingleOrDefault(x => x.Name == csvOverallPlayer.Chemistry) ??
                                        throw new Exception($"No chemistry found with value {csvOverallPlayer.Chemistry}"),
                            BatHandedness = batHandedness.SingleOrDefault(x => x.Name == csvOverallPlayer.BatHand) ??
                                            throw new Exception($"No bat handedness found with value {csvOverallPlayer.BatHand}"),
                            ThrowHandedness = throwHandedness.SingleOrDefault(x => x.Name == csvOverallPlayer.ThrowHand) ??
                                              throw new Exception($"No throw handedness found with value {csvOverallPlayer.ThrowHand}"),
                            PrimaryPosition = positions.SingleOrDefault(x => x.Name == csvOverallPlayer.Position) ??
                                              throw new Exception($"No position found with value {csvOverallPlayer.Position}"),
                            PitcherRole = !string.IsNullOrEmpty(csvOverallPlayer.PitcherRole)
                                ? pitcherRoles.SingleOrDefault(x => x.Name == csvOverallPlayer.PitcherRole)
                                : null,
                            GameIdHistory = new List<PlayerGameIdHistory>
                            {
                                new PlayerGameIdHistory
                                {
                                    GameId = csvOverallPlayer.PlayerId
                                }
                            }
                        };

                        _dbContext.Players.Add(player);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    player = playerGameIdHistory.Player;
                }

                // here, going to search a player season matching the season for the given player
                // if it does not exist, create it
                var newPlayerSeason = false;
                var playerSeason = await _dbContext.PlayerSeasons
                    .Include(x => x.GameStats)
                    .Include(x => x.TeamHistory)
                    .Include(x => x.SecondaryPosition)
                    .Include(x => x.Traits)
                    .SingleOrDefaultAsync(x => x.PlayerId == player.Id && x.SeasonId == csvOverallPlayer.SeasonId);

                if (playerSeason is null)
                {
                    newPlayerSeason = true;
                    playerSeason = new PlayerSeason
                    {
                        PlayerId = player.Id,
                        SeasonId = csvOverallPlayer.SeasonId,
                    };
                }

                playerSeason.Age = csvOverallPlayer.Age;
                playerSeason.Salary = csvOverallPlayer.Salary;
                playerSeason.SecondaryPosition = positions.SingleOrDefault(x => x.Name == csvOverallPlayer.SecondaryPosition) ??
                                                 throw new Exception($"No position found with value {csvOverallPlayer.SecondaryPosition}");

                var csvTraits = new List<string>();
                if (!string.IsNullOrEmpty(csvOverallPlayer.Trait1))
                    csvTraits.Add(csvOverallPlayer.Trait1);
                if (!string.IsNullOrEmpty(csvOverallPlayer.Trait2))
                    csvTraits.Add(csvOverallPlayer.Trait2);

                playerSeason.Traits = csvTraits
                    .Select(x => traits
                        .SingleOrDefault(y => y.Name == x) ?? throw new Exception($"No trait found with value {x}"))
                    .ToList();

                if (player.PitcherRole is not null)
                {
                    var csvPitches = new List<string>();
                    if (!string.IsNullOrEmpty(csvOverallPlayer.Pitch1))
                        csvPitches.Add(csvOverallPlayer.Pitch1);
                    if (!string.IsNullOrEmpty(csvOverallPlayer.Pitch2))
                        csvPitches.Add(csvOverallPlayer.Pitch2);
                    if (!string.IsNullOrEmpty(csvOverallPlayer.Pitch3))
                        csvPitches.Add(csvOverallPlayer.Pitch3);
                    if (!string.IsNullOrEmpty(csvOverallPlayer.Pitch4))
                        csvPitches.Add(csvOverallPlayer.Pitch4);
                    if (!string.IsNullOrEmpty(csvOverallPlayer.Pitch5))
                        csvPitches.Add(csvOverallPlayer.Pitch5);

                    playerSeason.PitchTypes = csvPitches
                        .Select(x => pitchTypes
                            .SingleOrDefault(y => y.Name == x) ?? throw new Exception($"No pitch type found with value {x}"))
                        .ToList();
                }

                playerSeason.SecondaryPosition = positions.SingleOrDefault(x => x.Name == csvOverallPlayer.SecondaryPosition);

                if (newPlayerSeason)
                    _dbContext.PlayerSeasons.Add(playerSeason);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return new Success();
    }

    // Import step #3
    public async Task<OneOf<Success, Exception>> AddPlayerPitchingStatsAsync(List<CsvPitchingStat> pitchingStats)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return e;
        }
        return new Success();
    }

    // Import step #4
    public async Task<OneOf<Success, Exception>> AddPlayerBattingStatsAsync(List<CsvBattingStat> battingStats)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return e;
        }
        return new Success();
    }

    public async Task<OneOf<Success, Exception>> AddSeasonScheduleAsync(List<CsvSeasonSchedule> schedule)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return e;
        }
        return new Success();
    }
    
    public async Task<OneOf<Success, Exception>> AddPlayoffScheduleAsync(List<CsvPlayoffSchedule> schedule)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return e;
        }
        return new Success();
    }
}