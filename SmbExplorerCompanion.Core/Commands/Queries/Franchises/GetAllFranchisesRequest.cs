using MediatR;
using SmbExplorerCompanion.Core.Entities;

namespace SmbExplorerCompanion.Core.Commands.Queries.Franchises;

public class GetAllFranchisesRequest : IRequest<FranchiseDto>
{
    // ReSharper disable once UnusedType.Global
    public class GetAllFranchisesHandler : IRequestHandler<GetAllFranchisesRequest, FranchiseDto>
    {
        public async Task<FranchiseDto> Handle(GetAllFranchisesRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}