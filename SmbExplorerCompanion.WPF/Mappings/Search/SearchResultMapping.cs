using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.WPF.Models.Search;

namespace SmbExplorerCompanion.WPF.Mappings.Search;

[Mapper]
public partial class SearchResultMapping
{
    public partial SearchResult FromSearchResultDto(SearchResultDto searchResultDto);
}