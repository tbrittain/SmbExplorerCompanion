using SmbExplorerCompanion.Core.Entities.Search;

namespace SmbExplorerCompanion.WPF.Models.Search;

public class SearchResult
{
    public SearchResultType Type { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int Id { get; init; }
}