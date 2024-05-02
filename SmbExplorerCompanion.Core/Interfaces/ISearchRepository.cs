using SmbExplorerCompanion.Core.Entities.Search;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISearchRepository
{
    public Task<IEnumerable<SearchResultDto>> Search(string query, CancellationToken cancellationToken);
}