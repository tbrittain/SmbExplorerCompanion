namespace SmbExplorerCompanion.Core.Entities.Search;

public class SearchResultDto
{
    public SearchResultType Type { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Id { get; set; }
}