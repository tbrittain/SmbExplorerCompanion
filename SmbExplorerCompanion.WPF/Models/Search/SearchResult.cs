using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Search;

public record SearchResult : LookupBase
{
    public SearchResultType Type { get; init; }
    public string Description { get; init; } = default!;
}

public static class SearchResultExtensions
{
    public static SearchResult FromCore(this SearchResultDto searchResultDto)
    {
        return new SearchResult
        {
            Id = searchResultDto.Id,
            Name = searchResultDto.Name,
            Description = searchResultDto.Description,
            Type = searchResultDto.Type
        };
    }
}