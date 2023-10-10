using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Summary;

public class GetFranchiseSummaryRequest : IRequest<OneOf<FranchiseSummaryDto, None, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetFranchiseSummaryHandler : IRequestHandler<GetFranchiseSummaryRequest, OneOf<FranchiseSummaryDto, None, Exception>>
    {
        private readonly ISummaryRepository _summaryRepository;

        public GetFranchiseSummaryHandler(ISummaryRepository summaryRepository)
        {
            _summaryRepository = summaryRepository;
        }

        public async Task<OneOf<FranchiseSummaryDto, None, Exception>> Handle(GetFranchiseSummaryRequest request,
            CancellationToken cancellationToken) => await _summaryRepository.GetFranchiseSummaryAsync(cancellationToken);
    }
}