using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class SummaryRepository : ISummaryRepository
{
    private readonly SmbExplorerCompanionDbContext _context;
    private readonly IApplicationContext _applicationContext;
    private readonly IPitcherCareerRepository _pitcherCareerRepository;
    private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;

    public SummaryRepository(SmbExplorerCompanionDbContext context,
        IApplicationContext applicationContext,
        IPitcherCareerRepository pitcherCareerRepository,
        IPositionPlayerCareerRepository positionPlayerCareerRepository)
    {
        _context = context;
        _applicationContext = applicationContext;
        _pitcherCareerRepository = pitcherCareerRepository;
        _positionPlayerCareerRepository = positionPlayerCareerRepository;
    }

    public Task<bool> HasFranchiseDataAsync(CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        return _context.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .AnyAsync(cancellationToken: cancellationToken);
    }

    public async Task<FranchiseSummaryDto?> GetFranchiseSummaryAsync(CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var franchiseSeasons = await _context.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .OrderByDescending(x => x.Number)
            .ToListAsync(cancellationToken: cancellationToken);

        // This indicates that no data has been imported yet
        if (!franchiseSeasons.Any()) return null;

        var franchiseSummaryDto = new FranchiseSummaryDto
        {
            NumSeasons = franchiseSeasons.Count
        };

        var playersIQueryable = _context.Players
            .Where(x => x.FranchiseId == franchiseId);

        var numPlayers = await playersIQueryable
            .CountAsync(cancellationToken: cancellationToken);

        franchiseSummaryDto.NumPlayers = numPlayers;

        var mostRecentSeason = franchiseSeasons
            .OrderByDescending(x => x.Number)
            .First();

        franchiseSummaryDto.MostRecentSeasonNumber = mostRecentSeason.Number;

        var numHallOfFamers = await playersIQueryable
            .Where(x => x.IsHallOfFamer)
            .CountAsync(cancellationToken: cancellationToken);

        franchiseSummaryDto.NumHallOfFamers = numHallOfFamers;

        var mostRecentChampionTeam = await _context.SeasonTeamHistory
            .Include(x => x.Team)
            .Include(x => x.TeamNameHistory)
            .Where(x => x.Team.FranchiseId == franchiseId)
            .Where(x => x.SeasonId == mostRecentSeason.Id)
            .Where(x => x.ChampionshipWinner != null)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mostRecentChampionTeam is not null)
        {
            franchiseSummaryDto.MostRecentChampionTeamId = mostRecentChampionTeam.Team.Id;
            franchiseSummaryDto.MostRecentChampionTeamName = mostRecentChampionTeam.TeamNameHistory.Name;
        }

        var mostRecentSeasonAwardees = _context.PlayerSeasons
            .Include(x => x.Awards)
            .Include(x => x.Player)
            .Where(x => x.SeasonId == mostRecentSeason.Id);

        var mvpAward = await _context.PlayerAwards
            .Where(x => x.OriginalName == "MVP")
            .SingleAsync(cancellationToken: cancellationToken);

        var mostRecentSeasonMvp = await mostRecentSeasonAwardees
            .Where(x => x.Awards.Any(y => y.Id == mvpAward.Id))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mostRecentSeasonMvp is not null)
        {
            franchiseSummaryDto.MostRecentMvpPlayerId = mostRecentSeasonMvp.PlayerId;
            franchiseSummaryDto.MostRecentMvpPlayerName = $"{mostRecentSeasonMvp.Player.FirstName} {mostRecentSeasonMvp.Player.LastName}";
        }

        var cyYoungAward = await _context.PlayerAwards
            .Where(x => x.OriginalName == "Cy Young")
            .SingleAsync(cancellationToken: cancellationToken);

        var mostRecentSeasonCyYoung = await mostRecentSeasonAwardees
            .Where(x => x.Awards.Any(y => y.Id == cyYoungAward.Id))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mostRecentSeasonCyYoung is not null)
        {
            franchiseSummaryDto.MostRecentCyYoungPlayerId = mostRecentSeasonCyYoung.PlayerId;
            franchiseSummaryDto.MostRecentCyYoungPlayerName =
                $"{mostRecentSeasonCyYoung.Player.FirstName} {mostRecentSeasonCyYoung.Player.LastName}";
        }

        // Get some top player leaders
        var topHomeRuns = await playersIQueryable
            .Select(x => new PlayerLeaderSummaryDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                StatName = "Home Runs",
                StatValue = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HomeRuns)),
            })
            .OrderByDescending(x => x.StatValue)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (topHomeRuns is not null)
        {
            franchiseSummaryDto.TopHomeRuns = topHomeRuns;
        }

        var topHits = await playersIQueryable
            .Select(x => new PlayerLeaderSummaryDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                StatName = "Hits",
                StatValue = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Hits)),
            })
            .OrderByDescending(x => x.StatValue)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (topHits is not null)
        {
            franchiseSummaryDto.TopHits = topHits;
        }

        var topRunsBattedIn = await playersIQueryable
            .Select(x => new PlayerLeaderSummaryDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                StatName = "Runs Batted In",
                StatValue = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.RunsBattedIn)),
            })
            .OrderByDescending(x => x.StatValue)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (topRunsBattedIn is not null)
        {
            franchiseSummaryDto.TopRunsBattedIn = topRunsBattedIn;
        }

        var topWins = await playersIQueryable
            .Select(x => new PlayerLeaderSummaryDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                StatName = "Wins",
                StatValue = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Wins)),
            })
            .OrderByDescending(x => x.StatValue)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (topWins is not null)
        {
            franchiseSummaryDto.TopWins = topWins;
        }

        var topSaves = await playersIQueryable
            .Select(x => new PlayerLeaderSummaryDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                StatName = "Saves",
                StatValue = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Saves)),
            })
            .OrderByDescending(x => x.StatValue)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (topSaves is not null)
        {
            franchiseSummaryDto.TopSaves = topSaves;
        }

        var topStrikeouts = await playersIQueryable
            .Select(x => new PlayerLeaderSummaryDto
            {
                PlayerId = x.Id,
                PlayerName = $"{x.FirstName} {x.LastName}",
                StatName = "Strikeouts",
                StatValue = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Strikeouts)),
            })
            .OrderByDescending(x => x.StatValue)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (topStrikeouts is not null)
        {
            franchiseSummaryDto.TopStrikeouts = topStrikeouts;
        }

        var currentBattingGreats = await _positionPlayerCareerRepository.GetBattingCareers(
            limit: 5,
            onlyActivePlayers: true,
            cancellationToken: cancellationToken);

        var currentPitchingGreats = await _pitcherCareerRepository.GetPitchingCareers(
            limit: 5,
            onlyActivePlayers: true,
            cancellationToken: cancellationToken);

        var currentGreats = currentBattingGreats
            .Concat(currentPitchingGreats.Cast<PlayerCareerBaseDto>())
            .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
            .ToList();

        franchiseSummaryDto.CurrentGreats = currentGreats;

        return franchiseSummaryDto;
    }

    public async Task<List<ConferenceSummaryDto>> GetLeagueSummaryAsync(CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var mostRecentSeason = await _context.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (mostRecentSeason is null) return new List<ConferenceSummaryDto>();

        var maxPlayoffSeries = await _context.GetMaxPlayoffSeriesAsync(franchiseId, cancellationToken);

        var conferences = await _context.Conferences
            .Where(x => x.FranchiseId == franchiseId)
            .ToListAsync(cancellationToken: cancellationToken);

        var conferenceSummaryDtos = conferences
            .Select(x => new ConferenceSummaryDto
            {
                Id = x.Id,
                ConferenceName = x.Name,
            })
            .ToList();

        foreach (var conferenceSummaryDto in conferenceSummaryDtos)
        {
            var divisions = await _context.Divisions
                .Where(x => x.ConferenceId == conferenceSummaryDto.Id)
                .ToListAsync(cancellationToken: cancellationToken);

            var divisionSummaryDtos = divisions
                .Select(x => new DivisionSummaryDto
                {
                    Id = x.Id,
                    DivisionName = x.Name,
                })
                .ToList();

            foreach (var divisionSummaryDto in divisionSummaryDtos)
            {
                var mostRecentSeasonTeamHistory = await _context.SeasonTeamHistory
                    .Include(x => x.TeamNameHistory)
                    .Include(x => x.ChampionshipWinner)
                    .Include(x => x.HomePlayoffSchedule)
                    .Include(x => x.AwayPlayoffSchedule)
                    .Where(x => x.SeasonId == mostRecentSeason.Id)
                    .Where(x => x.DivisionId == divisionSummaryDto.Id)
                    .ToListAsync(cancellationToken: cancellationToken);

                var mostRecentSeasonTeamHistoryDtos = mostRecentSeasonTeamHistory
                    .Select(x =>
                    {
                        var teamSummary =
                            new TeamSummaryDto
                            {
                                Id = x.TeamId,
                                SeasonTeamId = x.Id,
                                TeamName = x.TeamNameHistory.Name,
                                Wins = x.Wins,
                                Losses = x.Losses,
                                PlayoffSeed = x.PlayoffSeed,
                                PlayoffWins = x.PlayoffWins,
                                PlayoffLosses = x.PlayoffLosses,
                                IsDivisionChampion = x.GamesBehind == 0,
                            };

                        if (maxPlayoffSeries is not null)
                        {
                            teamSummary.IsConferenceChampion =
                                x.HomePlayoffSchedule
                                    .Any(y => y.SeriesNumber == maxPlayoffSeries) ||
                                x.AwayPlayoffSchedule
                                    .Any(y => y.SeriesNumber == maxPlayoffSeries);
                            teamSummary.IsChampion = x.ChampionshipWinner is not null;
                        }

                        return teamSummary;
                    })
                    .ToList();

                divisionSummaryDto.Teams = mostRecentSeasonTeamHistoryDtos;
            }

            conferenceSummaryDto.Divisions = divisionSummaryDtos;
        }

        return conferenceSummaryDtos;
    }
}