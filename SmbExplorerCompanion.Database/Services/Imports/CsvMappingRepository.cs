using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Progress;
using SmbExplorerCompanion.Csv.Models;
using SmbExplorerCompanion.Database.Entities;
using Team = SmbExplorerCompanion.Database.Entities.Team;

namespace SmbExplorerCompanion.Database.Services.Imports;

public class CsvMappingRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IApplicationContext _applicationContext;

    public CsvMappingRepository(SmbExplorerCompanionDbContext dbContext, IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    // Import step #1
    public async Task AddTeamsAsync(List<CsvTeam> teams,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;
        for (var i = 0; i < teams.Count; i++)
        {
            var csvTeam = teams[i];

            var conference = await _dbContext.Conferences
                .Where(x => x.FranchiseId == franchiseId)
                .SingleOrDefaultAsync(x => x.Name == csvTeam.ConferenceName, cancellationToken);
            if (conference is null)
            {
                conference = new Conference
                {
                    FranchiseId = franchiseId,
                    Name = csvTeam.ConferenceName
                };
                _dbContext.Conferences.Add(conference);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var division = await _dbContext.Divisions
                .Include(x => x.Conference)
                .Where(x => x.Conference.FranchiseId == franchiseId)
                .SingleOrDefaultAsync(x => x.Name == csvTeam.DivisionName, cancellationToken);
            if (division is null)
            {
                division = new Division
                {
                    ConferenceId = conference.Id,
                    Name = csvTeam.DivisionName
                };
                _dbContext.Divisions.Add(division);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            Team? team;

            var teamGameIdHistory = await _dbContext.TeamGameIdHistory
                .Include(x => x.Team)
                .ThenInclude(x => x.SeasonTeamHistory)
                .ThenInclude(x => x.TeamNameHistory)
                .SingleOrDefaultAsync(x => x.GameId == csvTeam.TeamId &&
                                           x.Team.FranchiseId == franchiseId,
                    cancellationToken);
            if (teamGameIdHistory is null)
            {
                // attempt to locate team based on the team name
                var teamNameHistory = await _dbContext.TeamNameHistory
                    .Include(x => x.SeasonTeamHistory)
                    .ThenInclude(x => x.Team)
                    .ThenInclude(x => x.SeasonTeamHistory)
                    .ThenInclude(x => x.TeamNameHistory)
                    .Include(x => x.SeasonTeamHistory)
                    .ThenInclude(x => x.Team)
                    .ThenInclude(x => x.TeamGameIdHistory)
                    .Include(x => x.SeasonTeamHistory)
                    .Where(x => x.SeasonTeamHistory.Any(y => y.Team.FranchiseId == franchiseId))
                    .SingleOrDefaultAsync(x => x.Name == csvTeam.TeamName &&
                                               x.SeasonTeamHistory.Any(y => y.Team.FranchiseId == franchiseId),
                        cancellationToken);

                // assume the team is new and does not exist if there is no equivalent team matching the name
                if (teamNameHistory is null)
                {
                    team = new Team
                    {
                        FranchiseId = franchiseId,
                        TeamGameIdHistory = new List<TeamGameIdHistory>
                        {
                            new()
                            {
                                GameId = csvTeam.TeamId
                            }
                        }
                    };
                    _dbContext.Teams.Add(team);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    team = teamNameHistory.SeasonTeamHistory.First().Team;
                    team.TeamGameIdHistory.Add(new TeamGameIdHistory
                    {
                        GameId = csvTeam.TeamId
                    });
                }
            }
            else
            {
                team = teamGameIdHistory.Team;
            }

            var newSeasonTeamHistory = false;
            var seasonTeamHistory = await _dbContext.SeasonTeamHistory
                .Include(x => x.Team)
                .Include(x => x.TeamNameHistory)
                .SingleOrDefaultAsync(x => x.TeamId == team.Id && x.SeasonId == season.Id, cancellationToken);

            if (seasonTeamHistory is null)
            {
                newSeasonTeamHistory = true;
                seasonTeamHistory = new SeasonTeamHistory
                {
                    SeasonId = season.Id,
                    TeamId = team.Id,
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
            var lastTeamNameHistory = team.SeasonTeamHistory.LastOrDefault()?.TeamNameHistory;
            if (lastTeamNameHistory?.Name != csvTeam.TeamName)
            {
                var teamNameHistory = new TeamNameHistory
                {
                    Name = csvTeam.TeamName
                };
                _dbContext.TeamNameHistory.Add(teamNameHistory);
                await _dbContext.SaveChangesAsync(cancellationToken);

                seasonTeamHistory.TeamNameHistoryId = teamNameHistory.Id;
            }
            else
            {
                seasonTeamHistory.TeamNameHistoryId = lastTeamNameHistory.Id;
            }

            if (newSeasonTeamHistory)
                team.SeasonTeamHistory.Add(seasonTeamHistory);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await channelWriter.WriteAsync(new ImportProgress
                {
                    CsvFileName = "Teams",
                    TotalRecords = teams.Count,
                    RecordNumber = i + 1
                },
                cancellationToken);
        }
    }

    private List<PlayerTeamHistory> GetPlayerTeamHistory(SeasonStatBase seasonStatBase, List<SeasonTeamHistory> seasonTeamHistories)
    {
        // overwrite the PlayerTeamHistory entries with what is present on the CSV
        var playerTeamHistories = new List<PlayerTeamHistory>();

        var currentOrder = 1;

        if (!seasonStatBase.CurrentTeamId.HasValue)
        {
            playerTeamHistories.Add(new PlayerTeamHistory
            {
                Order = currentOrder++,
                SeasonTeamHistoryId = null
            });
        }
        else
        {
            var currentTeam = seasonStatBase.CurrentTeamId!.Value;
            var currentTeamSeasonTeamHistory = seasonTeamHistories
                                                   .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                       .Any(y => y.GameId == currentTeam))
                                               ?? throw new Exception($"No SeasonTeamHistory record found for team ID {currentTeam}");

            playerTeamHistories.Add(new PlayerTeamHistory
            {
                Order = currentOrder++,
                SeasonTeamHistoryId = currentTeamSeasonTeamHistory.Id
            });
        }

        var mostRecentTeam = seasonStatBase.PreviousTeamId;
        if (mostRecentTeam.HasValue)
        {
            // If the player has a current team that differs from the most recent team, then we need to add a new team history
            if (mostRecentTeam != seasonStatBase.CurrentTeamId)
            {
                var mostRecentSeasonTeamHistory = seasonTeamHistories
                                                      .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                          .Any(y => y.GameId == mostRecentTeam))
                                                  ?? throw new Exception($"No SeasonTeamHistory record found for team ID {mostRecentTeam}");

                playerTeamHistories.Add(new PlayerTeamHistory
                {
                    Order = currentOrder++,
                    SeasonTeamHistoryId = mostRecentSeasonTeamHistory.Id
                });
            }
        }

        var secondMostRecentTeam = seasonStatBase.SecondPreviousTeamId;
        if (secondMostRecentTeam.HasValue)
        {
            var secondMostRecentTeamHistory = seasonTeamHistories
                                                  .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                      .Any(y => y.GameId == secondMostRecentTeam))
                                              ?? throw new Exception(
                                                  $"No SeasonTeamHistory record found for team ID {secondMostRecentTeam}");

            playerTeamHistories.Add(new PlayerTeamHistory
            {
                Order = currentOrder,
                SeasonTeamHistoryId = secondMostRecentTeamHistory.Id
            });
        }

        return playerTeamHistories;
    }

    // Import step #2
    public async Task AddOverallPlayersAsync(List<CsvOverallPlayer> players,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var batHandedness = await _dbContext.BatHandedness.ToListAsync(cancellationToken: cancellationToken);
        var throwHandedness = await _dbContext.ThrowHandedness.ToListAsync(cancellationToken: cancellationToken);
        var positions = await _dbContext.Positions.ToListAsync(cancellationToken: cancellationToken);
        var chemistry = await _dbContext.Chemistry.ToListAsync(cancellationToken: cancellationToken);
        var pitcherRoles = await _dbContext.PitcherRoles.ToListAsync(cancellationToken: cancellationToken);
        var traits = await _dbContext.Traits.ToListAsync(cancellationToken: cancellationToken);
        var pitchTypes = await _dbContext.PitchTypes.ToListAsync(cancellationToken: cancellationToken);

        int? previousSeasonId = null;
        if (season.Number != 1)
        {
            previousSeasonId = await _dbContext.Seasons
                .Where(x => x.FranchiseId == franchiseId)
                .Where(x => x.Number == season.Number - 1)
                .Select(x => x.Id)
                .SingleAsync(cancellationToken: cancellationToken);
        }

        for (var i = 0; i < players.Count; i++)
        {
            var csvOverallPlayer = players[i];
            Player? player;
            var playerGameIdHistory = await _dbContext.PlayerGameIdHistory
                .Include(x => x.Player)
                .ThenInclude(x => x.PlayerSeasons)
                .ThenInclude(x => x.GameStats)
                .Include(x => x.Player)
                .ThenInclude(x => x.PlayerSeasons)
                .ThenInclude(x => x.SecondaryPosition)
                .Include(x => x.Player)
                .ThenInclude(x => x.PlayerSeasons)
                .ThenInclude(x => x.Traits)
                .Where(x => x.Player.FranchiseId == franchiseId)
                .SingleOrDefaultAsync(x => x.GameId == csvOverallPlayer.PlayerId &&
                                           x.Player.FranchiseId == franchiseId,
                    cancellationToken);

            if (playerGameIdHistory is null)
            {
                // attempt to match on player name AND position AND pitcher role (if applicable) AND chemistry AND handedness
                // AND they played in the previous season. This should be enough to uniquely identify a player
                player = await _dbContext.Players
                    .Include(x => x.Chemistry)
                    .Include(x => x.BatHandedness)
                    .Include(x => x.ThrowHandedness)
                    .Include(x => x.PrimaryPosition)
                    .Include(x => x.PlayerGameIdHistory)
                    .Include(x => x.PitcherRole)
                    .Where(x => x.Chemistry!.Name == csvOverallPlayer.Chemistry
                                && x.BatHandedness.Name == csvOverallPlayer.BatHand
                                && x.ThrowHandedness.Name == csvOverallPlayer.ThrowHand
                                && x.PrimaryPosition.Name == csvOverallPlayer.Position
                                && (x.PitcherRole == null || x.PitcherRole.Name == csvOverallPlayer.PitcherRole)
                                && x.FirstName == csvOverallPlayer.FirstName
                                && x.LastName == csvOverallPlayer.LastName
                                && (previousSeasonId == null || x.PlayerSeasons.Any(y => y.SeasonId == previousSeasonId))
                                && x.FranchiseId == franchiseId)
                    .SingleOrDefaultAsync(cancellationToken: cancellationToken);

                if (player is null)
                {
                    // assume it is a new player if we cannot find a match
                    player = new Player
                    {
                        FranchiseId = franchiseId,
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
                        PlayerGameIdHistory = new List<PlayerGameIdHistory>
                        {
                            new()
                            {
                                GameId = csvOverallPlayer.PlayerId
                            }
                        }
                    };

                    _dbContext.Players.Add(player);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    player.PlayerGameIdHistory.Add(new PlayerGameIdHistory
                    {
                        GameId = csvOverallPlayer.PlayerId
                    });
                }
            }
            else
            {
                player = playerGameIdHistory.Player;
            }

            // here, going to search a player season matching the season for the given player
            // if it does not exist, create it
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.GameStats)
                .Include(x => x.PlayerTeamHistory)
                .Include(x => x.SecondaryPosition)
                .Include(x => x.Traits)
                .SingleOrDefaultAsync(x => x.PlayerId == player.Id && x.SeasonId == season.Id,
                    cancellationToken) ?? new PlayerSeason
            {
                PlayerId = player.Id,
                SeasonId = season.Id
            };

            playerSeason.Age = csvOverallPlayer.Age;
            playerSeason.Salary = csvOverallPlayer.Salary;
            playerSeason.SecondaryPosition =
                string.IsNullOrEmpty(csvOverallPlayer.SecondaryPosition)
                    ? null
                    : positions.SingleOrDefault(x => x.Name == csvOverallPlayer.SecondaryPosition) ??
                      throw new Exception($"No position found with value {csvOverallPlayer.SecondaryPosition}");

            var csvTraits = new List<string>();
            if (!string.IsNullOrEmpty(csvOverallPlayer.Trait1))
                csvTraits.Add(csvOverallPlayer.Trait1);
            if (!string.IsNullOrEmpty(csvOverallPlayer.Trait2))
                csvTraits.Add(csvOverallPlayer.Trait2);

            playerSeason.Traits = csvTraits
                .Select(x => traits
                    // TODO: SMB4 hardcoded here
                    .SingleOrDefault(y => y.Name == x && !y.IsSmb3) ?? throw new Exception($"No trait found with value {x}"))
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

            playerSeason.GameStats = new PlayerSeasonGameStat
            {
                Power = csvOverallPlayer.Power,
                Contact = csvOverallPlayer.Contact,
                Speed = csvOverallPlayer.Speed,
                Fielding = csvOverallPlayer.Fielding,
                Arm = csvOverallPlayer.Arm,
                Velocity = csvOverallPlayer.Velocity,
                Junk = csvOverallPlayer.Junk,
                Accuracy = csvOverallPlayer.Accuracy
            };

            if (playerSeason.Id == default)
                _dbContext.PlayerSeasons.Add(playerSeason);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await channelWriter.WriteAsync(new ImportProgress
                {
                    CsvFileName = "Overall Players",
                    TotalRecords = players.Count,
                    RecordNumber = i + 1
                },
                cancellationToken);
        }
    }

    // Import step #3
    public async Task AddPlayerPitchingStatsAsync(List<CsvPitchingStat> pitchingStats,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        bool isRegularSeason = true,
        CancellationToken cancellationToken = default)
    {
        var seasonTeamHistories = await _dbContext.SeasonTeamHistory
            .Include(x => x.Team)
            .ThenInclude(x => x.TeamGameIdHistory)
            .Where(x => x.SeasonId == season.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        for (var i = 0; i < pitchingStats.Count; i++)
        {
            var csvPitchingStat = pitchingStats[i];
            var playerGameIdHistory = await _dbContext.PlayerGameIdHistory
                                          .Include(x => x.Player)
                                          .ThenInclude(x => x.PlayerSeasons)
                                          .ThenInclude(x => x.PitchingStats)
                                          .Where(x => x.Player.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
                                          .SingleOrDefaultAsync(x => x.GameId == csvPitchingStat.PlayerId, cancellationToken)
                                      ?? throw new Exception($"No player found with the given player ID {csvPitchingStat.PlayerId}");

            var playerSeason = playerGameIdHistory.Player.PlayerSeasons
                                   .SingleOrDefault(x => x.SeasonId == season.Id)
                               ?? throw new Exception(
                                   $"No player season found for player ID {csvPitchingStat.PlayerId} and season ID {season.Id}");

            if (isRegularSeason)
            {
                var playerTeamHistories = GetPlayerTeamHistory(csvPitchingStat, seasonTeamHistories);
                playerSeason.PlayerTeamHistory = playerTeamHistories;
            }
            else
            {
                var playerTeamHistories = await _dbContext.PlayerSeasons
                    .Include(x => x.PlayerTeamHistory)
                    .ThenInclude(x => x.SeasonTeamHistory)
                    .ThenInclude(x => x!.Team)
                    .ThenInclude(x => x.TeamGameIdHistory)
                    .Where(x => x.PlayerId == playerSeason.PlayerId && x.SeasonId == season.Id)
                    .SelectMany(x => x.PlayerTeamHistory)
                    .ToListAsync(cancellationToken: cancellationToken);

                var mostRecentTeamHistory = playerTeamHistories
                    .SingleOrDefault(x => x.Order == 1) ?? throw new Exception(
                    $"No SeasonTeamHistory record found for team ID {csvPitchingStat.CurrentTeamId} " +
                    $"and season ID {season.Id}");

                // ensure that the team ID on the CSV import matches the team ID on Order 1 in the player team history
                if (mostRecentTeamHistory.SeasonTeamHistory is null)
                {
                    // This means that the last team that was recorded for this player was them being a free agent, and we need to update it
                    // to indicate that they were signed by a team for the postseason
                    mostRecentTeamHistory.SeasonTeamHistory = seasonTeamHistories
                                                                  .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                                      .Any(y => y.GameId == csvPitchingStat.CurrentTeamId))
                                                              ?? throw new Exception(
                                                                  $"No SeasonTeamHistory record found for team ID {csvPitchingStat.CurrentTeamId} " +
                                                                  $"and season ID {season.Id}");
                }
                else if (mostRecentTeamHistory.SeasonTeamHistory!.Team.TeamGameIdHistory
                         .All(x => x.GameId != csvPitchingStat.CurrentTeamId))
                {
                    // This means that the team ID on the CSV import does not match the team ID on Order 1 in the player team history
                    // We will then need to add a new player team history record to indicate that they were signed by a team for the postseason
                    // and also update the orders of the existing records
                    var newPlayerTeamHistory = new PlayerTeamHistory
                    {
                        Order = 1,
                        SeasonTeamHistory = seasonTeamHistories
                                                .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                    .Any(y => y.GameId == csvPitchingStat.CurrentTeamId))
                                            ?? throw new Exception(
                                                $"No SeasonTeamHistory record found for team ID {csvPitchingStat.CurrentTeamId} " +
                                                $"and season ID {season.Id}")
                    };

                    // update the orders of the existing records
                    foreach (var playerTeamHistory in playerTeamHistories)
                    {
                        playerTeamHistory.Order++;
                    }

                    playerTeamHistories.Add(newPlayerTeamHistory);
                }
            }

            // Here, add the pitching-specific stats. Attempt to find an existing record, and if it doesn't exist, create a new one
            // Also need to check depending on whether this is a regular season or playoff import
            var playerSeasonPitchingStat = playerSeason.PitchingStats
                .SingleOrDefault(x => x.IsRegularSeason == isRegularSeason) ?? new PlayerSeasonPitchingStat
            {
                IsRegularSeason = isRegularSeason
            };

            playerSeasonPitchingStat.Wins = csvPitchingStat.Wins;
            playerSeasonPitchingStat.Losses = csvPitchingStat.Losses;
            playerSeasonPitchingStat.CompleteGames = csvPitchingStat.CompleteGames;
            playerSeasonPitchingStat.Shutouts = csvPitchingStat.Shutouts;
            playerSeasonPitchingStat.Hits = csvPitchingStat.Hits;
            playerSeasonPitchingStat.EarnedRuns = csvPitchingStat.EarnedRuns;
            playerSeasonPitchingStat.HomeRuns = csvPitchingStat.HomeRuns;
            playerSeasonPitchingStat.Walks = csvPitchingStat.Walks;
            playerSeasonPitchingStat.Strikeouts = csvPitchingStat.Strikeouts;
            playerSeasonPitchingStat.InningsPitched = csvPitchingStat.InningsPitched;
            playerSeasonPitchingStat.EarnedRunAverage = csvPitchingStat.Era;
            playerSeasonPitchingStat.TotalPitches = csvPitchingStat.TotalPitches;
            playerSeasonPitchingStat.Saves = csvPitchingStat.Saves;
            playerSeasonPitchingStat.HitByPitch = csvPitchingStat.HitByPitch;
            playerSeasonPitchingStat.BattersFaced = csvPitchingStat.BattersFaced;
            playerSeasonPitchingStat.GamesPlayed = csvPitchingStat.GamesPlayed;
            playerSeasonPitchingStat.GamesStarted = csvPitchingStat.GamesStarted;
            playerSeasonPitchingStat.GamesFinished = csvPitchingStat.GamesFinished;
            playerSeasonPitchingStat.RunsAllowed = csvPitchingStat.RunsAllowed;
            playerSeasonPitchingStat.WildPitches = csvPitchingStat.WildPitches;
            playerSeasonPitchingStat.BattingAverageAgainst = csvPitchingStat.BattingAverageAgainst;
            playerSeasonPitchingStat.Fip = GetNormalizedDouble(csvPitchingStat.Fip);
            playerSeasonPitchingStat.Whip = GetNormalizedDouble(csvPitchingStat.Whip);
            playerSeasonPitchingStat.WinPercentage = GetNormalizedDouble(csvPitchingStat.WinPercentage);
            playerSeasonPitchingStat.OpponentObp = GetNormalizedDouble(csvPitchingStat.OpponentObp);
            playerSeasonPitchingStat.StrikeoutsPerWalk = GetNormalizedDouble(csvPitchingStat.StrikeoutsPerWalk);
            playerSeasonPitchingStat.StrikeoutsPerNine = GetNormalizedDouble(csvPitchingStat.StrikeoutsPerNine);
            playerSeasonPitchingStat.HitsPerNine = GetNormalizedDouble(csvPitchingStat.HitsPerNine);
            playerSeasonPitchingStat.HomeRunsPerNine = GetNormalizedDouble(csvPitchingStat.HomeRunsPerNine);
            playerSeasonPitchingStat.PitchesPerInning = GetNormalizedDouble(csvPitchingStat.PitchesPerInning);
            playerSeasonPitchingStat.PitchesPerGame = GetNormalizedDouble(csvPitchingStat.PitchesPerGame);
            playerSeasonPitchingStat.EraMinus = GetNormalizedDouble(csvPitchingStat.EraMinus);
            playerSeasonPitchingStat.FipMinus = GetNormalizedDouble(csvPitchingStat.FipMinus);

            if (playerSeasonPitchingStat.Id == default)
                playerSeason.PitchingStats.Add(playerSeasonPitchingStat);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await channelWriter.WriteAsync(new ImportProgress
                {
                    CsvFileName = "Pitching Stats",
                    TotalRecords = pitchingStats.Count,
                    RecordNumber = i + 1
                },
                cancellationToken);
        }
    }

    // Import step #4
    public async Task AddPlayerBattingStatsAsync(List<CsvBattingStat> battingStats,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        bool isRegularSeason = true,
        CancellationToken cancellationToken = default)
    {
        var seasonTeamHistories = await _dbContext.SeasonTeamHistory
            .Include(x => x.Team)
            .ThenInclude(x => x.TeamGameIdHistory)
            .Where(x => x.SeasonId == season.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        for (var i = 0; i < battingStats.Count; i++)
        {
            var csvBattingStat = battingStats[i];
            var playerGameIdHistory = await _dbContext.PlayerGameIdHistory
                                          .Include(x => x.Player)
                                          .ThenInclude(x => x.PlayerSeasons)
                                          .ThenInclude(x => x.BattingStats)
                                          .Where(x => x.Player.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
                                          .SingleOrDefaultAsync(x => x.GameId == csvBattingStat.PlayerId, cancellationToken)
                                      ?? throw new Exception($"No player found with the given player ID {csvBattingStat.PlayerId}");

            var playerSeason = playerGameIdHistory.Player.PlayerSeasons
                                   .SingleOrDefault(x => x.SeasonId == season.Id)
                               ?? throw new Exception(
                                   $"No player season found for player ID {csvBattingStat.PlayerId} and season ID {season.Id}");

            if (isRegularSeason)
            {
                var playerTeamHistories = GetPlayerTeamHistory(csvBattingStat, seasonTeamHistories);
                playerSeason.PlayerTeamHistory = playerTeamHistories;
            }
            else
            {
                var playerTeamHistories = await _dbContext.PlayerSeasons
                    .Include(x => x.PlayerTeamHistory)
                    .ThenInclude(x => x.SeasonTeamHistory)
                    .ThenInclude(x => x!.Team)
                    .ThenInclude(x => x.TeamGameIdHistory)
                    .Where(x => x.PlayerId == playerSeason.PlayerId && x.SeasonId == season.Id)
                    .SelectMany(x => x.PlayerTeamHistory)
                    .ToListAsync(cancellationToken: cancellationToken);

                var mostRecentTeamHistory = playerTeamHistories
                    .SingleOrDefault(x => x.Order == 1) ?? throw new Exception(
                    $"No SeasonTeamHistory record found for team ID {csvBattingStat.CurrentTeamId} " +
                    $"and season ID {season.Id}");

                // ensure that the team ID on the CSV import matches the team ID on Order 1 in the player team history
                if (mostRecentTeamHistory.SeasonTeamHistory is null)
                {
                    // This means that the last team that was recorded for this player was them being a free agent, and we need to update it
                    // to indicate that they were signed by a team for the postseason
                    mostRecentTeamHistory.SeasonTeamHistory = seasonTeamHistories
                                                                  .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                                      .Any(y => y.GameId == csvBattingStat.CurrentTeamId))
                                                              ?? throw new Exception(
                                                                  $"No SeasonTeamHistory record found for team ID {csvBattingStat.CurrentTeamId} " +
                                                                  $"and season ID {season.Id}");
                }
                else if (mostRecentTeamHistory.SeasonTeamHistory!.Team.TeamGameIdHistory
                         .All(x => x.GameId != csvBattingStat.CurrentTeamId))
                {
                    // This means that the team ID on the CSV import does not match the team ID on Order 1 in the player team history
                    // We will then need to add a new player team history record to indicate that they were signed by a team for the postseason
                    // and also update the orders of the existing records
                    var newPlayerTeamHistory = new PlayerTeamHistory
                    {
                        Order = 1,
                        SeasonTeamHistory = seasonTeamHistories
                                                .SingleOrDefault(x => x.Team.TeamGameIdHistory
                                                    .Any(y => y.GameId == csvBattingStat.CurrentTeamId))
                                            ?? throw new Exception(
                                                $"No SeasonTeamHistory record found for team ID {csvBattingStat.CurrentTeamId} " +
                                                $"and season ID {season.Id}")
                    };

                    // update the orders of the existing records
                    foreach (var playerTeamHistory in playerTeamHistories)
                    {
                        playerTeamHistory.Order++;
                    }

                    playerTeamHistories.Add(newPlayerTeamHistory);
                }
            }

            // Here, add the batting-specific stats. Attempt to find an existing record, and if it doesn't exist, create a new one
            // Also need to check depending on whether this is a regular season or playoff import

            var playerSeasonBattingStat = playerSeason.BattingStats
                .SingleOrDefault(x => x.IsRegularSeason == isRegularSeason) ?? new PlayerSeasonBattingStat
            {
                IsRegularSeason = isRegularSeason
            };

            playerSeasonBattingStat.GamesPlayed = csvBattingStat.GamesPlayed;
            playerSeasonBattingStat.GamesBatting = csvBattingStat.GamesBatting;
            playerSeasonBattingStat.AtBats = csvBattingStat.AtBats;
            playerSeasonBattingStat.PlateAppearances = csvBattingStat.PlateAppearances;
            playerSeasonBattingStat.Runs = csvBattingStat.Runs;
            playerSeasonBattingStat.Hits = csvBattingStat.Hits;
            playerSeasonBattingStat.Singles = csvBattingStat.Singles;
            playerSeasonBattingStat.Doubles = csvBattingStat.Doubles;
            playerSeasonBattingStat.Triples = csvBattingStat.Triples;
            playerSeasonBattingStat.HomeRuns = csvBattingStat.HomeRuns;
            playerSeasonBattingStat.RunsBattedIn = csvBattingStat.RunsBattedIn;
            playerSeasonBattingStat.ExtraBaseHits = csvBattingStat.ExtraBaseHits;
            playerSeasonBattingStat.TotalBases = csvBattingStat.TotalBases;
            playerSeasonBattingStat.StolenBases = csvBattingStat.StolenBases;
            playerSeasonBattingStat.CaughtStealing = csvBattingStat.CaughtStealing;
            playerSeasonBattingStat.Walks = csvBattingStat.Walks;
            playerSeasonBattingStat.Strikeouts = csvBattingStat.Strikeouts;
            playerSeasonBattingStat.HitByPitch = csvBattingStat.HitByPitch;
            playerSeasonBattingStat.Obp = GetNormalizedDouble(csvBattingStat.Obp);
            playerSeasonBattingStat.Slg = GetNormalizedDouble(csvBattingStat.Slg);
            playerSeasonBattingStat.Ops = GetNormalizedDouble(csvBattingStat.Ops);
            playerSeasonBattingStat.Woba = GetNormalizedDouble(csvBattingStat.Woba);
            playerSeasonBattingStat.Iso = GetNormalizedDouble(csvBattingStat.Iso);
            playerSeasonBattingStat.Babip = GetNormalizedDouble(csvBattingStat.Babip);
            playerSeasonBattingStat.SacrificeHits = csvBattingStat.SacrificeHits;
            playerSeasonBattingStat.SacrificeFlies = csvBattingStat.SacrificeFlies;
            playerSeasonBattingStat.BattingAverage = GetNormalizedDouble(csvBattingStat.BattingAverage);
            playerSeasonBattingStat.Errors = csvBattingStat.Errors;
            playerSeasonBattingStat.PassedBalls = csvBattingStat.PassedBalls;
            playerSeasonBattingStat.PaPerGame = GetNormalizedDouble(csvBattingStat.PaPerGame);
            playerSeasonBattingStat.AbPerHomeRun = GetNormalizedDouble(csvBattingStat.AbPerHomeRun);
            playerSeasonBattingStat.StrikeoutPercentage = GetNormalizedDouble(csvBattingStat.StrikeoutPercentage);
            playerSeasonBattingStat.WalkPercentage = GetNormalizedDouble(csvBattingStat.WalkPercentage);
            playerSeasonBattingStat.ExtraBaseHitPercentage = GetNormalizedDouble(csvBattingStat.ExtraBaseHitPercentage);
            playerSeasonBattingStat.OpsPlus = GetNormalizedDouble(csvBattingStat.OpsPlus);

            if (playerSeasonBattingStat.Id == default)
                playerSeason.BattingStats.Add(playerSeasonBattingStat);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await channelWriter.WriteAsync(new ImportProgress
                {
                    CsvFileName = "Batting Stats",
                    TotalRecords = battingStats.Count,
                    RecordNumber = i + 1
                },
                cancellationToken);
        }
    }

    private static double? GetNormalizedDouble(double? value)
    {
        return value is null
            ? null
            : double.IsNaN(value.Value) || double.IsInfinity(value.Value)
                ? null
                : value;
    }

    public async Task AddSeasonScheduleAsync(List<CsvSeasonSchedule> schedule,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var seasonTeamHistories = await _dbContext.SeasonTeamHistory
            .Include(x => x.Team)
            .ThenInclude(x => x.TeamGameIdHistory)
            .Include(seasonTeamHistory => seasonTeamHistory.HomeSeasonSchedule)
            .Include(seasonTeamHistory => seasonTeamHistory.AwaySeasonSchedule)
            .Where(x => x.SeasonId == season.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        var pitchers = await _dbContext.PlayerSeasons
            .Include(x => x.Player)
            .ThenInclude(x => x.PlayerGameIdHistory)
            .Where(x => x.SeasonId == season.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        var numberOfGamesInSeason = schedule.Max(x => x.Day);
        season.NumGamesRegularSeason = numberOfGamesInSeason;

        // reset the season schedule for all teams in case we are re-importing it
        foreach (var seasonTeamHistory in seasonTeamHistories)
        {
            foreach (var teamSeasonSchedule in seasonTeamHistory.HomeSeasonSchedule
                         .Union(seasonTeamHistory.AwaySeasonSchedule))
            {
                _dbContext.TeamSeasonSchedules.Remove(teamSeasonSchedule);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        for (var i = 0; i < schedule.Count; i++)
        {
            var csvGameSchedule = schedule[i];
            var homeTeam =
                seasonTeamHistories
                    .SingleOrDefault(x => x.Team.TeamGameIdHistory
                        .Any(y => y.GameId == csvGameSchedule.HomeTeamId))
                ?? throw new Exception($"No SeasonTeamHistory record found for team ID {csvGameSchedule.HomeTeamId} " +
                                       $"and season ID {season.Id}");

            var awayTeam =
                seasonTeamHistories
                    .SingleOrDefault(x => x.Team.TeamGameIdHistory
                        .Any(y => y.GameId == csvGameSchedule.AwayTeamId))
                ?? throw new Exception($"No SeasonTeamHistory record found for team ID {csvGameSchedule.AwayTeamId} " +
                                       $"and season ID {season.Id}");

            var newScheduleEntry = new TeamSeasonSchedule
            {
                HomeTeamHistory = homeTeam,
                AwayTeamHistory = awayTeam,
                Day = csvGameSchedule.Day,
                GlobalGameNumber = csvGameSchedule.GlobalGameNumber
            };

            var isGamePlayed = csvGameSchedule.HomeScore is not null && csvGameSchedule.AwayScore is not null;
            if (isGamePlayed)
            {
                newScheduleEntry.HomeScore = csvGameSchedule.HomeScore;
                newScheduleEntry.AwayScore = csvGameSchedule.AwayScore;

                var homePitcherSeason =
                    pitchers
                        .SingleOrDefault(x => x.Player.PlayerGameIdHistory
                            .Any(y => y.GameId == csvGameSchedule.HomePitcherId))
                    ?? throw new Exception($"No pitcher found for player ID {csvGameSchedule.HomePitcherId} " +
                                           $"and season ID {season.Id}");

                var awayPitcherSeason =
                    pitchers
                        .SingleOrDefault(x => x.Player.PlayerGameIdHistory
                            .Any(y => y.GameId == csvGameSchedule.AwayPitcherId))
                    ?? throw new Exception($"No pitcher found for player ID {csvGameSchedule.AwayPitcherId} " +
                                           $"and season ID {season.Id}");

                newScheduleEntry.HomePitcherSeason = homePitcherSeason;
                newScheduleEntry.AwayPitcherSeason = awayPitcherSeason;
            }

            _dbContext.TeamSeasonSchedules.Add(newScheduleEntry);

            await channelWriter.WriteAsync(new ImportProgress
                {
                    CsvFileName = "Season Schedule",
                    TotalRecords = schedule.Count,
                    RecordNumber = i + 1
                },
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPlayoffScheduleAsync(List<CsvPlayoffSchedule> schedule,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var seasonTeamHistories = await _dbContext.SeasonTeamHistory
            .Include(x => x.Team)
            .ThenInclude(x => x.TeamGameIdHistory)
            .Include(x => x.HomePlayoffSchedule)
            .Include(x => x.AwayPlayoffSchedule)
            .Where(x => x.SeasonId == season.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        var pitchers = await _dbContext.PlayerSeasons
            .Include(x => x.Player)
            .ThenInclude(x => x.PlayerGameIdHistory)
            .Where(x => x.SeasonId == season.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        // reset the playoff schedule for all teams in case we are re-importing it
        foreach (var seasonTeamHistory in seasonTeamHistories)
        {
            foreach (var teamPlayoffSchedule in seasonTeamHistory.HomePlayoffSchedule
                         .Union(seasonTeamHistory.AwayPlayoffSchedule))
            {
                _dbContext.TeamPlayoffSchedules.Remove(teamPlayoffSchedule);
            }

            var potentialTeamIds = seasonTeamHistory.Team.TeamGameIdHistory
                .Select(x => x.GameId)
                .ToList();

            var madePlayoffs = schedule
                .Any(x => potentialTeamIds.Contains(x.HomeTeamId) || potentialTeamIds.Contains(x.AwayTeamId));
            if (madePlayoffs)
            {
                var teamGameId = schedule
                    .Where(x => potentialTeamIds.Contains(x.HomeTeamId))
                    .Select(x => x.HomeTeamId)
                    .First();

                // Try to get the team's seed from the Team1
                var teamSeed = schedule
                    .Where(x => x.Team1Id == teamGameId)
                    .Select(x => x.Team1Seed)
                    .FirstOrDefault();

                // Fall back to Team2
                if (teamSeed == default)
                {
                    teamSeed = schedule
                        .Where(x => x.Team2Id == teamGameId)
                        .Select(x => x.Team2Seed)
                        .FirstOrDefault();

                    if (teamSeed == default)
                        throw new Exception($"No team seed found for team ID {seasonTeamHistory.Team.Id} and season ID {season.Id}");
                }

                seasonTeamHistory.PlayoffSeed = teamSeed;

                var homeGames = schedule
                    .Where(x => x.HomeTeamId == teamGameId)
                    .ToList();

                var awayGames = schedule
                    .Where(x => x.AwayTeamId == teamGameId)
                    .ToList();

                seasonTeamHistory.PlayoffWins = homeGames.Count(x => x.HomeScore > x.AwayScore) +
                                                awayGames.Count(x => x.AwayScore > x.HomeScore);
                seasonTeamHistory.PlayoffLosses = homeGames.Count(x => x.HomeScore < x.AwayScore) +
                                                  awayGames.Count(x => x.AwayScore < x.HomeScore);

                seasonTeamHistory.PlayoffRunsScored = homeGames.Sum(x => x.HomeScore) +
                                                      awayGames.Sum(x => x.AwayScore);
                seasonTeamHistory.PlayoffRunsAllowed = homeGames.Sum(x => x.AwayScore) +
                                                       awayGames.Sum(x => x.HomeScore);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        for (var i = 0; i < schedule.Count; i++)
        {
            var csvPlayoffSchedule = schedule[i];
            var homeTeam =
                seasonTeamHistories
                    .SingleOrDefault(x => x.Team.TeamGameIdHistory
                        .Any(y => y.GameId == csvPlayoffSchedule.HomeTeamId))
                ?? throw new Exception($"No SeasonTeamHistory record found for team ID {csvPlayoffSchedule.HomeTeamId} " +
                                       $"and season ID {season.Id}");

            var awayTeam =
                seasonTeamHistories
                    .SingleOrDefault(x => x.Team.TeamGameIdHistory
                        .Any(y => y.GameId == csvPlayoffSchedule.AwayTeamId))
                ?? throw new Exception($"No SeasonTeamHistory record found for team ID {csvPlayoffSchedule.AwayTeamId} " +
                                       $"and season ID {season.Id}");

            var newScheduleEntry = new TeamPlayoffSchedule
            {
                HomeTeamHistory = homeTeam,
                AwayTeamHistory = awayTeam,
                SeriesNumber = csvPlayoffSchedule.SeriesNum,
                GlobalGameNumber = csvPlayoffSchedule.GlobalGameNumber
            };

            var isGamePlayed = csvPlayoffSchedule.HomeScore is not null && csvPlayoffSchedule.AwayScore is not null;
            if (isGamePlayed)
            {
                newScheduleEntry.HomeScore = csvPlayoffSchedule.HomeScore;
                newScheduleEntry.AwayScore = csvPlayoffSchedule.AwayScore;

                var homePitcherSeason =
                    pitchers
                        .SingleOrDefault(x => x.Player.PlayerGameIdHistory
                            .Any(y => y.GameId == csvPlayoffSchedule.HomePitcherId))
                    ?? throw new Exception($"No pitcher found for player ID {csvPlayoffSchedule.HomePitcherId} " +
                                           $"and season ID {season.Id}");

                var awayPitcherSeason =
                    pitchers
                        .SingleOrDefault(x => x.Player.PlayerGameIdHistory
                            .Any(y => y.GameId == csvPlayoffSchedule.AwayPitcherId))
                    ?? throw new Exception($"No pitcher found for player ID {csvPlayoffSchedule.AwayPitcherId} " +
                                           $"and season ID {season.Id}");

                newScheduleEntry.HomePitcherSeason = homePitcherSeason;
                newScheduleEntry.AwayPitcherSeason = awayPitcherSeason;
            }

            _dbContext.TeamPlayoffSchedules.Add(newScheduleEntry);

            await channelWriter.WriteAsync(new ImportProgress
                {
                    CsvFileName = "Playoff Schedule",
                    TotalRecords = schedule.Count,
                    RecordNumber = i + 1
                },
                cancellationToken);
        }

        var isAnyGameNotPlayed = schedule.Any(x => x.HomeScore is null || x.AwayScore is null);
        if (!isAnyGameNotPlayed)
        {
            // The Playoffs should have completed at this point, so we can add the ChampionshipWinner record
            var maxSeriesNumber = schedule.Max(x => x.SeriesNum);

            var winningTeamIds = schedule.Where(x => x.SeriesNum == maxSeriesNumber)
                .Select(championshipGames => championshipGames.HomeScore > championshipGames.AwayScore
                    ? championshipGames.HomeTeamId
                    : championshipGames.AwayTeamId)
                .ToList();

            var championshipWinnerTeamId = winningTeamIds
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .First();

            var championshipWinnerTeam =
                seasonTeamHistories
                    .SingleOrDefault(x => x.Team.TeamGameIdHistory
                        .Any(y => y.GameId == championshipWinnerTeamId.Key))
                ?? throw new Exception($"No SeasonTeamHistory record found for team ID {championshipWinnerTeamId.Key} " +
                                       $"and season ID {season.Id}");

            var championshipWinner = new ChampionshipWinner
            {
                Season = season,
                SeasonTeamHistory = championshipWinnerTeam
            };

            var playerSeasons = await _dbContext.PlayerSeasons
                .Include(x => x.PlayerTeamHistory)
                .ThenInclude(y => y.SeasonTeamHistory)
                .Where(x => x.PlayerTeamHistory
                    .Any(y => y.SeasonTeamHistory != null &&
                              y.SeasonTeamHistory.Id == championshipWinnerTeam.Id &&
                              y.Order == 1))
                .Where(x => x.SeasonId == season.Id)
                .ToListAsync(cancellationToken: cancellationToken);

            championshipWinner.PlayerSeasons = playerSeasons;
            season.ChampionshipWinner = championshipWinner;

            _dbContext.ChampionshipWinners.Add(championshipWinner);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}