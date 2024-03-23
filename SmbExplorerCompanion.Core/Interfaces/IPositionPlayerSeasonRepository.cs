﻿using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.ValueObjects.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public record GetBattingSeasonsFilters : PlayerFilters
{
    public int? PrimaryPositionId { get; init; } = null;
    public bool OnlyRookies { get; init; } = false;
    public bool IncludeChampionAwards { get; init; } = true;
    public bool OnlyUserAssignableAwards { get; init; } = false;
    public int? TeamId { get; init; } = null;
    public bool IsPlayoffs { get; init; } = false;
}

public interface IPositionPlayerSeasonRepository
{
    public Task<List<PlayerBattingSeasonDto>> GetBattingSeasons(
        GetBattingSeasonsFilters filters,
        CancellationToken cancellationToken = default);
}