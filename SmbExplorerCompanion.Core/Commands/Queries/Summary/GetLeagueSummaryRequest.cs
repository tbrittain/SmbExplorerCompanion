using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Summary;

public class GetLeagueSummaryRequest : IRequest<OneOf<List<ConferenceSummaryDto>, None, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetLeagueSummaryHandler : IRequestHandler<GetLeagueSummaryRequest, OneOf<List<ConferenceSummaryDto>, None, Exception>>
    {
        private readonly ISummaryRepository _summaryRepository;

        public GetLeagueSummaryHandler(ISummaryRepository summaryRepository)
        {
            _summaryRepository = summaryRepository;
        }

        public async Task<OneOf<List<ConferenceSummaryDto>, None, Exception>> Handle(GetLeagueSummaryRequest request,
            CancellationToken cancellationToken) =>
            await _summaryRepository.GetLeagueSummaryAsync(cancellationToken);
    }
}