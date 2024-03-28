using MediatR;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Summary;

public class GetHasFranchiseDataRequest : IRequest<bool>
{
    // ReSharper disable once UnusedType.Global
    internal class GetHasFranchiseDataHandler : IRequestHandler<GetHasFranchiseDataRequest, bool>
    {
        private readonly ISummaryRepository _summaryRepository;

        public GetHasFranchiseDataHandler(ISummaryRepository summaryRepository)
        {
            _summaryRepository = summaryRepository;
        }

        public async Task<bool> Handle(GetHasFranchiseDataRequest request, CancellationToken cancellationToken)
        {
            return await _summaryRepository.HasFranchiseDataAsync(cancellationToken);
        }
    }
}