﻿using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPositionPlayerSeasonRepository
{
    public Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> GetBattingSeasons(
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
        CancellationToken cancellationToken = default);
}