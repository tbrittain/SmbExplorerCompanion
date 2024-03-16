﻿using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;
using SmbExplorerCompanion.Shared.Enums;
using static SmbExplorerCompanion.Shared.Constants.WeightedOpsPlusOrEraMinus;

namespace SmbExplorerCompanion.Database.Services.Players;

public class PositionPlayerSeasonRepository : IPositionPlayerSeasonRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IApplicationContext _applicationContext;

    public PositionPlayerSeasonRepository(SmbExplorerCompanionDbContext dbContext, IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    public async Task<List<PlayerBattingSeasonDto>> GetBattingSeasons(
        SeasonRange? seasons = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        int? primaryPositionId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false,
        int? playerId = null,
        CancellationToken cancellationToken = default)
    {
        if (onlyRookies && seasons is null)
            throw new ArgumentException("SeasonRange must be provided if OnlyRookies is true");

        if (playerId is not null && pageNumber is not null)
            throw new ArgumentException("Cannot provide both PlayerId and PageNumber");

        var limitToTeam = teamId is not null;
        if (orderBy is not null)
        {
            orderBy += descending ? " desc" : " asc";
        }
        else
        {
            const string defaultOrderByProperty = nameof(PlayerBattingSeasonDto.WeightedOpsPlusOrEraMinus);
            orderBy = $"{defaultOrderByProperty} desc";
        }

        var limitValue = limit ?? 30;

        var minSeasonId = await _dbContext.Seasons
            .Where(x => x.FranchiseId == _applicationContext.SelectedFranchiseId!.Value)
            .MinAsync(x => x.Id, cancellationToken);

        if (seasons?.StartSeasonId == minSeasonId && seasons?.EndSeasonId is not null) onlyRookies = false;

        List<int> rookiePlayerIds = new();
        if (onlyRookies)
            rookiePlayerIds = await _dbContext.Players
                .Where(x => x.PlayerSeasons.Any(p => p.SeasonId == seasons!.Value.StartSeasonId))
                .Select(x => new
                {
                    PlayerId = x.Id,
                    FirstSeasonId = x.PlayerSeasons.OrderBy(ps => ps.SeasonId).First().SeasonId
                })
                .Where(x => x.FirstSeasonId == seasons!.Value.StartSeasonId)
                .Select(x => x.PlayerId)
                .ToListAsync(cancellationToken: cancellationToken);

        var playerBattingDtos = await _dbContext.PlayerSeasonBattingStats
            .Include(x => x.PlayerSeason)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Traits)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Awards)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Player)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Season)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.PlayerTeamHistory)
            .ThenInclude(x => x.SeasonTeamHistory)
            .ThenInclude(x => x!.TeamNameHistory)
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.ChampionshipWinner)
            .Where(x => seasons == null || (x.PlayerSeason.SeasonId >= seasons.Value.StartSeasonId &&
                                            x.PlayerSeason.SeasonId <= seasons.Value.EndSeasonId))
            .Where(x => x.IsRegularSeason == !isPlayoffs)
            .Where(x => x.PlayerSeason.PlayerTeamHistory.Any(y => !limitToTeam ||
                                                                  (y.SeasonTeamHistory != null && y.SeasonTeamHistory.TeamId == teamId)))
            .Where(x => primaryPositionId == null || x.PlayerSeason.Player.PrimaryPositionId == primaryPositionId)
            .Where(x => !onlyRookies || rookiePlayerIds.Contains(x.PlayerSeason.PlayerId))
            .Where(x => playerId == null || x.PlayerSeason.PlayerId == playerId)
            .Select(x => new PlayerBattingSeasonDto
            {
                PlayerId = x.PlayerSeason.PlayerId,
                PlayerName = $"{x.PlayerSeason.Player.FirstName} {x.PlayerSeason.Player.LastName}",
                TeamNames = string.Join(", ",
                    x.PlayerSeason.PlayerTeamHistory
                        .OrderBy(y => y.Order)
                        .Where(y => y.SeasonTeamHistory != null)
                        .Select(y => y.SeasonTeamHistory!.TeamNameHistory.Name)),
                IsPitcher = x.PlayerSeason.Player.PitcherRoleId != null,
                TotalSalary = x.PlayerSeason.PlayerTeamHistory
                    .Single(y => y.Order == 1).SeasonTeamHistoryId == null
                    ? 0
                    : x.PlayerSeason.Salary,
                BatHandednessId = x.PlayerSeason.Player.BatHandednessId,
                ThrowHandednessId = x.PlayerSeason.Player.ThrowHandednessId,
                PrimaryPositionId = x.PlayerSeason.Player.PrimaryPositionId,
                PitcherRoleId = x.PlayerSeason.Player.PitcherRoleId,
                ChemistryId = x.PlayerSeason.Player.ChemistryId,
                SeasonId = x.PlayerSeason.SeasonId,
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
                WeightedOpsPlusOrEraMinus = ((x.OpsPlus ?? 0) - 95) * x.PlateAppearances * BattingScalingFactor +
                                            (x.StolenBases - x.CaughtStealing) * BaserunningScalingFactor,
                AwardIds = x.PlayerSeason.Awards
                    .Where(y => !onlyUserAssignableAwards || y.IsUserAssignable)
                    .Select(y => y.Id)
                    .ToList(),
                IsChampion = x.PlayerSeason.ChampionshipWinner != null,
                Age = x.PlayerSeason.Age,
                GamesBatting = x.GamesBatting,
                GamesPlayed = x.GamesPlayed,
                PlateAppearances = x.PlateAppearances,
                CaughtStealing = x.CaughtStealing,
                TotalBases = x.TotalBases,
                SecondaryPositionId = x.PlayerSeason.SecondaryPositionId,
                TraitIds = x.PlayerSeason.Traits
                    .Select(y => y.Id)
                    .ToList()
            })
            .OrderBy(orderBy)
            .Skip(((pageNumber ?? 1) - 1) * limitValue)
            .Take(limitValue)
            .ToListAsync(cancellationToken: cancellationToken);

        if (includeChampionAwards)
        {
            foreach (var player in playerBattingDtos.Where(player => player.IsChampion))
            {
                player.AwardIds.Add((int)VirtualAward.Champion);
            }
        }

        return playerBattingDtos;
    }
}