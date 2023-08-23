using SmbExplorerCompanion.Core.Entities.Search;
using OneOf;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISearchRepository
{
    public Task<OneOf<IEnumerable<SearchResultDto>, Exception>> Search(string query, CancellationToken cancellationToken);
}