﻿using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPitcherSeasonRepository
{
    public Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> GetPitchingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false,
        int? playerId = null,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);
}