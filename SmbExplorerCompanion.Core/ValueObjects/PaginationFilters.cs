namespace SmbExplorerCompanion.Core.ValueObjects;

public abstract record PaginationFilters
{
    public int? PageNumber { get; init; }
    public string? OrderBy { get; init; }
    public int? Limit { get; init; }
    public bool Descending { get; init; } = true;
}