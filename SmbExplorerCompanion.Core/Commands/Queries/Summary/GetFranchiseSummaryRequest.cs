using MediatR;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Summary;

public class GetFranchiseSummaryRequest : IRequest<FranchiseSummaryDto?>
{
    // ReSharper disable once UnusedType.Global
    internal class GetFranchiseSummaryHandler : IRequestHandler<GetFranchiseSummaryRequest, FranchiseSummaryDto?>
    {
        private readonly ISummaryRepository _summaryRepository;

        public GetFranchiseSummaryHandler(ISummaryRepository summaryRepository)
        {
            _summaryRepository = summaryRepository;
        }

        public async Task<FranchiseSummaryDto?> Handle(GetFranchiseSummaryRequest request,
            CancellationToken cancellationToken) => await _summaryRepository.GetFranchiseSummaryAsync(cancellationToken);
    }
}