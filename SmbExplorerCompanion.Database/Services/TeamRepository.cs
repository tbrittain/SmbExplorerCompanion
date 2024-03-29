﻿using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.Database.Entities;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services;

public class TeamRepository : ITeamRepository
{
    private readonly IApplicationContext _applicationContext;
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IPitcherSeasonRepository _pitcherSeasonRepository;
    private readonly IPositionPlayerSeasonRepository _positionPlayerSeasonRepository;

    public TeamRepository(SmbExplorerCompanionDbContext dbContext,
        IApplicationContext applicationContext,
        IPositionPlayerSeasonRepository positionPlayerSeasonRepository,
        IPitcherSeasonRepository pitcherSeasonRepository)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
        _positionPlayerSeasonRepository = positionPlayerSeasonRepository;
        _pitcherSeasonRepository = pitcherSeasonRepository;
    }

    public async Task<IEnumerable<TeamDto>> GetSeasonTeams(int seasonId, CancellationToken cancellationToken)
    {
        var teams = await _dbContext.SeasonTeamHistory
            .Include(x => x.Season)
            .Include(x => x.TeamNameHistory)
            .Include(x => x.Division)
            .ThenInclude(x => x.Conference)
            .Where(x => x.SeasonId == seasonId)
            .Select(x => new TeamDto
            {
                SeasonId = x.SeasonId,
                SeasonNumber = x.Season.Number,
                SeasonTeamId = x.Id,
                TeamId = x.TeamId,
                TeamName = x.TeamNameHistory.Name,
                DivisionName = x.Division.Name,
                ConferenceName = x.Division.Conference.Name
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return teams;
    }

    public async Task<IEnumerable<HistoricalTeamDto>> GetHistoricalTeams(SeasonRange seasonRange,
        CancellationToken cancellationToken)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;
        var teamsQueryable = _dbContext.SeasonTeamHistory
            .Include(x => x.Team)
            .Include(x => x.Division)
            .ThenInclude(x => x.Conference)
            .Where(x => x.Team.FranchiseId == franchiseId)
            .Where(x => x.SeasonId >= seasonRange.StartSeasonId && x.SeasonId <= seasonRange.EndSeasonId);

        var maxPlayoffSeries = await _dbContext.GetMaxPlayoffSeriesAsync(franchiseId, cancellationToken);

        var isMultipleSeasons = seasonRange.StartSeasonId != seasonRange.EndSeasonId;
        int? previousSeasonId = null;
        if (!isMultipleSeasons)
        {
            var previousSeason = await _dbContext.Seasons
                .Where(x => x.FranchiseId == franchiseId)
                .OrderByDescending(x => x.Number)
                .FirstOrDefaultAsync(x => x.Number < seasonRange.StartSeasonId, cancellationToken);

            previousSeasonId = previousSeason?.Id;
        }

        var teams = await teamsQueryable
            .Include(x => x.TeamNameHistory)
            .Include(x => x.PlayerTeamHistory)
            .ThenInclude(x => x.PlayerSeason)
            .ThenInclude(x => x.BattingStats)
            .Include(x => x.PlayerTeamHistory)
            .ThenInclude(x => x.PlayerSeason)
            .ThenInclude(x => x.PitchingStats)
            .Include(x => x.HomePlayoffSchedule)
            .Include(x => x.AwayPlayoffSchedule)
            .Include(x => x.ChampionshipWinner)
            .GroupBy(x => x.TeamId)
            .Select(x => new
            {
                TeamId = x.Key,
                SeasonTeamId = isMultipleSeasons
                    ? (int?) null
                    : x.First().Id,
                CurrentTeamName = x
                    .OrderByDescending(y => y.SeasonId)
                    .First().TeamNameHistory.Name,
                NumGames = x.Sum(y => y.Wins + y.Losses + (y.PlayoffWins ?? 0) + (y.PlayoffLosses ?? 0)),
                NumRegularSeasonWins = x.Sum(y => y.Wins),
                WinDiffFromPrevSeason = previousSeasonId != null
                    ? x
                        .Where(y => y.SeasonId == seasonRange.EndSeasonId)
                        .Sum(y => y.Wins) - x
                        .Where(y => y.SeasonId == previousSeasonId)
                        .Sum(y => y.Wins)
                    : 0,
                NumRegularSeasonLosses = x.Sum(y => y.Losses),
                NumPlayoffWins = x.Sum(y => y.PlayoffWins ?? 0),
                NumPlayoffLosses = x.Sum(y => y.PlayoffLosses ?? 0),
                NumDivisionsWon = x.Count(y => y.GamesBehind == 0),
                NumChampionships = x.Count(y => y.ChampionshipWinner != null),
                NumPlayoffAppearances = x.Count(y => y.HomePlayoffSchedule.Any() || y.AwayPlayoffSchedule.Any()),
                NumRunsScored = x.Sum(y => y.RunsScored + (y.PlayoffRunsScored ?? 0)),
                NumRunsAllowed = x.Sum(y => y.RunsAllowed + (y.PlayoffRunsAllowed ?? 0)),
                NumConferenceTitles = x
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
            var playerTeamHistory = await _dbContext.SeasonTeamHistory
                .Include(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.PlayerSeason)
                .ThenInclude(x => x.Player)
                .Include(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.PlayerSeason)
                .ThenInclude(x => x.BattingStats)
                .Include(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.PlayerSeason)
                .ThenInclude(x => x.PitchingStats)
                .Where(x => x.TeamId == team.TeamId)
                .Where(x => x.SeasonId >= seasonRange.StartSeasonId && x.SeasonId <= seasonRange.EndSeasonId)
                .SelectMany(y => y.PlayerTeamHistory)
                .ToListAsync(cancellationToken: cancellationToken);

            playerTeamHistories.Add(team.TeamId, playerTeamHistory);
        }

        var historicalTeams = teams
            .Select(x =>
            {
                var team = new HistoricalTeamDto
                {
                    TeamId = x.TeamId,
                    SeasonTeamId = x.SeasonTeamId,
                    CurrentTeamName = x.CurrentTeamName,
                    NumGames = x.NumGames,
                    WinDiffFromPrevSeason = x.WinDiffFromPrevSeason,
                    NumRegularSeasonWins = x.NumRegularSeasonWins,
                    NumRegularSeasonLosses = x.NumRegularSeasonLosses,
                    NumPlayoffWins = x.NumPlayoffWins,
                    NumPlayoffLosses = x.NumPlayoffLosses,
                    NumDivisionsWon = x.NumDivisionsWon,
                    NumChampionships = x.NumChampionships,
                    NumConferenceTitles = x.NumConferenceTitles,
                    NumPlayoffAppearances = x.NumPlayoffAppearances,
                    NumRunsScored = x.NumRunsScored,
                    NumRunsAllowed = x.NumRunsAllowed
                };

                var teamPlayers = playerTeamHistories[x.TeamId];

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
            .ToList();

        var currentSeason = await _dbContext.Seasons
            .Where(x => x.FranchiseId == franchiseId)
            .Where(x => x.Id == seasonRange.EndSeasonId)
            .OrderByDescending(x => x.Number)
            .FirstAsync(cancellationToken: cancellationToken);

        foreach (var team in historicalTeams)
        {
            var lastChampionshipSeason = await _dbContext.SeasonTeamHistory
                .Include(x => x.Season)
                .Include(x => x.ChampionshipWinner)
                .Where(x => x.TeamId == team.TeamId)
                .Where(x => x.ChampionshipWinner != null)
                .Where(x => x.Season.Number <= currentSeason.Number)
                .OrderByDescending(x => x.SeasonId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var championshipDroughtSeasons = currentSeason.Number - (lastChampionshipSeason?.Season.Number ?? 0);
            team.ChampionshipDroughtSeasons = championshipDroughtSeasons;
        }

        if (cancellationToken.IsCancellationRequested)
            return Array.Empty<HistoricalTeamDto>();

        return historicalTeams;
    }

    public async Task<TeamOverviewDto> GetTeamOverview(int teamId, CancellationToken cancellationToken)
    {
        var maxPlayoffSeries = await _dbContext
            .GetMaxPlayoffSeriesAsync(_applicationContext.SelectedFranchiseId!.Value, cancellationToken);

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
                NumChampionships = x.Count(y => y.ChampionshipWinner != null)
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
                WonChampionship = x.ChampionshipWinner != null
            })
            .OrderByDescending(x => x.SeasonNumber)
            .ToList();

        // TODO: This will likely pull back more data the more seasons there are,
        // so we will want to do some sort of ordering and limiting here in the future
        var topPlayers = await _dbContext.Players
            .Include(x => x.PlayerSeasons
                .Where(y => y.PlayerTeamHistory
                    .Any(z => z.SeasonTeamHistory != null && z.SeasonTeamHistory.TeamId == teamId)))
            .ThenInclude(x => x.Awards)
            .Include(x => x.PrimaryPosition)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.PlayerTeamHistory)
            .ThenInclude(x => x.SeasonTeamHistory)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.BattingStats)
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.PitchingStats)
            .Include(player => player.PlayerSeasons)
            .ThenInclude(x => x.ChampionshipWinner)
            .Include(player => player.PlayerSeasons)
            .ThenInclude(x => x.Season)
            .Where(x => x.PlayerSeasons
                .Any(y => y.PlayerTeamHistory
                    .Any(z => z.SeasonTeamHistory != null && z.SeasonTeamHistory.TeamId == teamId)))
            .ToListAsync(cancellationToken: cancellationToken);

        teamOverviewDto.TopPlayers = topPlayers
            .Select(x =>
            {
                var dto = new TeamTopPlayerHistoryDto();
                dto.PlayerId = x.Id;
                dto.PlayerName = $"{x.FirstName} {x.LastName}";
                dto.PlayerPosition = x.PrimaryPosition.Name;

                var seasonsWithTeam = x.PlayerSeasons
                    .Where(y => y.PlayerTeamHistory
                        .Any(z => z.SeasonTeamHistory is not null && z.SeasonTeamHistory.TeamId == teamId))
                    .ToList();
                dto.NumSeasonsWithTeam = seasonsWithTeam.Count;
                dto.SeasonNumbers = seasonsWithTeam
                    .Select(y => y.Season.Number)
                    .ToList();

                var isPitcher = x.PitcherRoleId is not null;
                dto.IsPitcher = isPitcher;

                dto.Awards = seasonsWithTeam
                    .SelectMany(y => y.Awards)
                    .Select(y => new PlayerAwardBaseDto
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Importance = y.Importance,
                        OmitFromGroupings = y.OmitFromGroupings
                    })
                    .ToList();

                if (x.IsHallOfFamer)
                    dto.Awards.Add(new PlayerAwardDto
                    {
                        Id = -1,
                        Importance = -1,
                        Name = "Hall of Fame",
                        OriginalName = "Hall of Fame",
                        OmitFromGroupings = false
                    });

                var numChampionships = seasonsWithTeam
                    .Count(y => y.ChampionshipWinner is not null);

                if (numChampionships > 0)
                    foreach (var _ in Enumerable.Range(1, numChampionships))
                    {
                        dto.Awards.Add(new PlayerAwardDto
                        {
                            Id = 0,
                            Name = "Champion",
                            Importance = 10,
                            OmitFromGroupings = false
                        });
                    }

                if (isPitcher)
                {
                    var pitchingStats = seasonsWithTeam
                        .SelectMany(y => y.PitchingStats)
                        .ToList();

                    if (pitchingStats.Any())
                    {
                        dto.AverageEraMinus = pitchingStats
                            .Average(y => y.EraMinus ?? 0);

                        dto.WeightedOpsPlusOrEraMinus = pitchingStats
                            .Sum(y => (((y.EraMinus ?? 0) + (y.FipMinus ?? 0)) / 2 - 95) * (y.InningsPitched ?? 0) * PitchingScalingFactor);
                    }
                }
                else
                {
                    var battingStats = seasonsWithTeam
                        .SelectMany(y => y.BattingStats)
                        .ToList();

                    if (battingStats.Any())
                    {
                        dto.AverageOpsPlus = battingStats
                            .Average(y => y.OpsPlus ?? 0);
                        dto.WeightedOpsPlusOrEraMinus = battingStats
                            .Sum(y => ((y.OpsPlus ?? 0) - 95) * y.PlateAppearances * BattingScalingFactor +
                                      (y.StolenBases - y.CaughtStealing) * BaserunningScalingFactor);
                    }
                }

                return dto;
            })
            .OrderByDescending(x => x.WeightedOpsPlusOrEraMinus)
            .Take(25)
            .ToList();

        return teamOverviewDto;
    }

    public async Task<TeamSeasonDetailDto> GetTeamSeasonDetail(int teamSeasonId, CancellationToken cancellationToken)
    {
        var maxPlayoffSeries = await _dbContext
            .GetMaxPlayoffSeriesAsync(_applicationContext.SelectedFranchiseId!.Value, cancellationToken);

        var teamSeason = await _dbContext.SeasonTeamHistory
            .Include(x => x.TeamNameHistory)
            .Include(x => x.Division)
            .ThenInclude(x => x.Conference)
            .Include(x => x.Season)
            .Include(x => x.ChampionshipWinner)
            .Include(x => x.HomePlayoffSchedule)
            .Include(x => x.AwayPlayoffSchedule)
            .Where(x => x.Id == teamSeasonId)
            .SingleAsync(cancellationToken: cancellationToken);

        var seasonId = teamSeason.SeasonId;
        var teamId = teamSeason.TeamId;

        var seasonPlayoffsCompleted = await _dbContext.ChampionshipWinners
            .AnyAsync(x => x.SeasonId == seasonId, cancellationToken);

        var teamSeasonDetailDto = new TeamSeasonDetailDto
        {
            TeamId = teamSeason.TeamId,
            CurrentTeamName = teamSeason.TeamNameHistory.Name,
            DivisionName = teamSeason.Division.Name,
            ConferenceName = teamSeason.Division.Conference.Name,
            SeasonNum = teamSeason.Season.Number,
            Budget = teamSeason.Budget,
            Payroll = teamSeason.Payroll,
            Surplus = teamSeason.Surplus,
            SurplusPerGame = teamSeason.SurplusPerGame,
            Wins = teamSeason.Wins,
            Losses = teamSeason.Losses,
            RunsScored = teamSeason.RunsScored,
            RunsAllowed = teamSeason.RunsAllowed,
            RunDifferential = teamSeason.RunsScored - teamSeason.RunsAllowed,
            GamesBehind = teamSeason.GamesBehind,
            WinPercentage = teamSeason.WinPercentage,
            PythagoreanWinPercentage = teamSeason.PythagoreanWinPercentage,
            ExpectedWins = teamSeason.ExpectedWins,
            ExpectedLosses = teamSeason.ExpectedLosses,
            TotalPower = teamSeason.TotalPower,
            TotalContact = teamSeason.TotalContact,
            TotalSpeed = teamSeason.TotalSpeed,
            TotalFielding = teamSeason.TotalFielding,
            TotalArm = teamSeason.TotalArm,
            TotalVelocity = teamSeason.TotalVelocity,
            TotalJunk = teamSeason.TotalJunk,
            TotalAccuracy = teamSeason.TotalAccuracy,
            MadePlayoffs = teamSeason.PlayoffWins > 0 || teamSeason.PlayoffLosses > 0,
            PlayoffSeed = teamSeason.PlayoffSeed,
            WonConference = teamSeason.HomePlayoffSchedule.Any(y => y.SeriesNumber == maxPlayoffSeries) ||
                            teamSeason.AwayPlayoffSchedule.Any(y => y.SeriesNumber == maxPlayoffSeries),
            WonChampionship = teamSeason.ChampionshipWinner is not null
        };

        if (teamSeasonDetailDto.MadePlayoffs && seasonPlayoffsCompleted)
        {
            teamSeasonDetailDto.IncludesPlayoffData = true;

            teamSeasonDetailDto.PlayoffResults = await GetTeamPlayoffResults(
                maxPlayoffSeries!.Value,
                teamSeason.HomePlayoffSchedule,
                teamSeason.AwayPlayoffSchedule);

            teamSeasonDetailDto.PlayoffPitching = await _pitcherSeasonRepository.GetPitchingSeasons(
                new GetPitchingSeasonsFilters
                {
                    Seasons = new SeasonRange(seasonId),
                    IsPlayoffs = true,
                    TeamId = teamId
                },
                cancellationToken);

            teamSeasonDetailDto.PlayoffBatting = await _positionPlayerSeasonRepository.GetBattingSeasons(
                new GetBattingSeasonsFilters
                {
                    Seasons = new SeasonRange(seasonId),
                    IsPlayoffs = true,
                    TeamId = teamId
                },
                cancellationToken);
        }

        teamSeasonDetailDto.RegularSeasonPitching = await _pitcherSeasonRepository.GetPitchingSeasons(
            new GetPitchingSeasonsFilters
            {
                Seasons = new SeasonRange(seasonId),
                IsPlayoffs = false,
                TeamId = teamId
            },
            cancellationToken);

        teamSeasonDetailDto.RegularSeasonBatting = await _positionPlayerSeasonRepository.GetBattingSeasons(
            new GetBattingSeasonsFilters
            {
                Seasons = new SeasonRange(seasonId),
                IsPlayoffs = false,
                TeamId = teamId
            },
            cancellationToken);

        return teamSeasonDetailDto;
    }

    public async Task<DivisionScheduleBreakdownDto> GetTeamScheduleBreakdown(int teamSeasonId,
        bool includeDivision,
        CancellationToken cancellationToken)
    {
        var seasonId = await _dbContext.SeasonTeamHistory
            .Where(x => x.Id == teamSeasonId)
            .Select(x => x.SeasonId)
            .SingleAsync(cancellationToken: cancellationToken);
        var teamSeasonIds = new List<int>(teamSeasonId);
        if (includeDivision)
        {
            var divisionId = await _dbContext.SeasonTeamHistory
                .Where(x => x.Id == teamSeasonId)
                .Select(x => x.DivisionId)
                .SingleAsync(cancellationToken: cancellationToken);

            teamSeasonIds = await _dbContext.SeasonTeamHistory
                .Include(x => x.Division)
                .Where(x => x.SeasonId == seasonId)
                .Where(x => x.DivisionId == divisionId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken: cancellationToken);
            teamSeasonIds = teamSeasonIds.Distinct().ToList();
        }

        var divisionScheduleBreakdown = new DivisionScheduleBreakdownDto();
        foreach (var currentTeamSeasonId in teamSeasonIds)
        {
            var teamSeason = await _dbContext.SeasonTeamHistory
                .Include(x => x.TeamNameHistory)
                .Include(x => x.HomeSeasonSchedule)
                .ThenInclude(x => x.AwayTeamHistory)
                .ThenInclude(x => x.TeamNameHistory)
                .Include(x => x.AwaySeasonSchedule)
                .ThenInclude(x => x.HomeTeamHistory)
                .ThenInclude(x => x.TeamNameHistory)
                .Where(x => x.Id == currentTeamSeasonId)
                .SingleAsync(cancellationToken: cancellationToken);

            var gamesPlayed = teamSeason.HomeSeasonSchedule
                .Concat(teamSeason.AwaySeasonSchedule)
                .Where(x => x is {HomeScore: not null, AwayScore: not null})
                .ToList();

            var teamScheduleBreakdowns = gamesPlayed
                .Select(x =>
                {
                    if (x.HomeTeamHistoryId == currentTeamSeasonId)
                        return new TeamScheduleBreakdownDto(
                            x.HomeTeamHistoryId,
                            x.HomeTeamHistory.TeamNameHistory.Name,
                            x.AwayTeamHistoryId,
                            x.AwayTeamHistory.TeamNameHistory.Name,
                            x.Day,
                            x.GlobalGameNumber,
                            x.HomeScore!.Value,
                            x.AwayScore!.Value);

                    return new TeamScheduleBreakdownDto(
                        x.AwayTeamHistoryId,
                        x.AwayTeamHistory.TeamNameHistory.Name,
                        x.HomeTeamHistoryId,
                        x.HomeTeamHistory.TeamNameHistory.Name,
                        x.Day,
                        x.GlobalGameNumber,
                        x.AwayScore!.Value,
                        x.HomeScore!.Value);
                })
                .OrderBy(x => x.Day)
                .ToHashSet();

            divisionScheduleBreakdown.TeamScheduleBreakdowns.Add(teamScheduleBreakdowns);
        }

        return divisionScheduleBreakdown;
    }

    // We will only call this method if the playoffs completed so that we do not need to return partial playoff completion results
    private async Task<List<TeamPlayoffRoundResultDto>> GetTeamPlayoffResults(int maxPlayoffSeries,
        IEnumerable<TeamPlayoffSchedule> homePlayoffSchedule,
        IEnumerable<TeamPlayoffSchedule> awayPlayoffSchedule)
    {
        List<TeamPlayoffGameResult> gameResults = new();
        var homeGameResults = homePlayoffSchedule.Select(homePlayoffGame => new TeamPlayoffGameResult(
                homePlayoffGame.AwayTeamHistoryId,
                homePlayoffGame.SeriesNumber,
                homePlayoffGame.GlobalGameNumber,
                true,
                homePlayoffGame.HomeScore!.Value,
                homePlayoffGame.AwayScore!.Value))
            .ToList();
        gameResults.AddRange(homeGameResults);

        var awayGameResults = awayPlayoffSchedule.Select(awayPlayoffGame => new TeamPlayoffGameResult(
                awayPlayoffGame.HomeTeamHistoryId,
                awayPlayoffGame.SeriesNumber,
                awayPlayoffGame.GlobalGameNumber,
                false,
                awayPlayoffGame.HomeScore!.Value,
                awayPlayoffGame.AwayScore!.Value))
            .ToList();
        gameResults.AddRange(awayGameResults);

        var gameResultsBySeries = gameResults
            .OrderBy(x => x.GameNumber)
            .GroupBy(x => x.SeriesNumber)
            .ToList();

        // This should never throw. If it does, we will need to revisit the retrieval of the max playoff series.
        // That may mean that incomplete playoff results are exported from SMBExplorer
        var applicableSeriesTypes = PlayoffSeries.SeriesLengths[maxPlayoffSeries];

        List<TeamPlayoffRoundResultDto> playoffResults = new();
        foreach (var series in gameResultsBySeries)
        {
            var seriesNumber = series.Key;
            var seriesType = applicableSeriesTypes
                .Where(x => x.MaxSeriesNumber >= seriesNumber)
                .OrderBy(x => x.MaxSeriesNumber)
                .First()
                .Round;

            var numWins = series
                .Count(x => (x.IsHomeTeam && x.HomeScore > x.AwayScore) ||
                            (!x.IsHomeTeam && x.AwayScore > x.HomeScore));
            var numLosses = series
                .Count(x => (x.IsHomeTeam && x.HomeScore < x.AwayScore) ||
                            (!x.IsHomeTeam && x.AwayScore < x.HomeScore));
            var teamWonSeries = numWins > numLosses;
            var opponentTeamSeasonId = series.First().OpponentTeamSeasonId;

            var opponentSeasonTeamHistory = await _dbContext.SeasonTeamHistory
                .Include(x => x.TeamNameHistory)
                .Where(x => x.Id == opponentTeamSeasonId)
                .SingleAsync();

            var opponentSeasonTeamId = opponentSeasonTeamHistory.Id;
            var opponentTeamName = opponentSeasonTeamHistory.TeamNameHistory.Name;

            var playoffRoundResult = new TeamPlayoffRoundResultDto
            {
                SeriesNumber = seriesNumber,
                Round = seriesType,
                OpponentSeasonTeamId = opponentSeasonTeamId,
                OpponentTeamName = opponentTeamName,
                WonSeries = teamWonSeries,
                NumWins = numWins,
                NumLosses = numLosses
            };
            playoffResults.Add(playoffRoundResult);
        }

        return playoffResults;
    }

    private record struct TeamPlayoffGameResult(
        int OpponentTeamSeasonId,
        int SeriesNumber,
        int GameNumber,
        bool IsHomeTeam,
        int HomeScore,
        int AwayScore);
}