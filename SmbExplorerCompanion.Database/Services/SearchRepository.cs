using OneOf;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class SearchRepository : ISearchRepository
{
    private SmbExplorerCompanionDbContext _context;

    public SearchRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public Task<OneOf<IEnumerable<SearchResult>, Exception>> Search(string query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}