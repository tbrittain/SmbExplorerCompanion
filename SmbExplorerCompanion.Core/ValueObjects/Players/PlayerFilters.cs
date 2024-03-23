using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.ValueObjects.Players;

public abstract record PlayerFilters : PaginationFilters
{
    public SeasonRange? Seasons { get; init; }
    public int? PlayerId { get; init; }
}