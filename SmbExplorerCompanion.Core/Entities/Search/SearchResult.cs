namespace SmbExplorerCompanion.Core.Entities.Search;

public class SearchResult
{
    public SearchResultType Type { get; set; }
    public string Name { get; set; } = default!;
    public int Id { get; set; }
}