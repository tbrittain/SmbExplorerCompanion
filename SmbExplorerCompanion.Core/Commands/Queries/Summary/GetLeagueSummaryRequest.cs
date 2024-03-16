using MediatR;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Summary;

public class GetLeagueSummaryRequest : IRequest<List<ConferenceSummaryDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetLeagueSummaryHandler : IRequestHandler<GetLeagueSummaryRequest, List<ConferenceSummaryDto>>
    {
        private readonly ISummaryRepository _summaryRepository;

        public GetLeagueSummaryHandler(ISummaryRepository summaryRepository)
        {
            _summaryRepository = summaryRepository;
        }

        public async Task<List<ConferenceSummaryDto>> Handle(GetLeagueSummaryRequest request,
            CancellationToken cancellationToken) =>
            await _summaryRepository.GetLeagueSummaryAsync(cancellationToken);
    }
}