using MediatR;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Search;

public class GetSearchResultsQuery : IRequest<IEnumerable<SearchResultDto>>
{
    public GetSearchResultsQuery(string searchQuery)
    {
        SearchQuery = searchQuery;
    }

    private string SearchQuery { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetSearchResultsHandler : IRequestHandler<GetSearchResultsQuery, IEnumerable<SearchResultDto>>
    {
        private readonly ISearchRepository _searchRepository;

        public GetSearchResultsHandler(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        public async Task<IEnumerable<SearchResultDto>> Handle(GetSearchResultsQuery request,
            CancellationToken cancellationToken)
        {
            return await _searchRepository.Search(request.SearchQuery, cancellationToken);
        }
    }
}