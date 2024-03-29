﻿using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.ValueObjects.Players;

public abstract record PlayerFilters : PaginationFilters
{
    public SeasonRange? Seasons { get; init; }
    public int? PlayerId { get; init; }
    public int? ChemistryId { get; init; }
    public int? BatHandednessId { get; init; }
    public int? ThrowHandednessId { get; init; }
    public bool IsPlayoffs { get; init; } = false;
    public bool OnlyQualifiedPlayers { get; init; } = false;
}

public abstract record SeasonPlayerFilters : PlayerFilters
{
    public int? SecondaryPositionId { get; init; }
    public List<int> TraitIds { get; init; } = new();
}

public abstract record CareerPlayerFilters : PlayerFilters;