using MediatR;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Franchises;

public class GetAllFranchisesRequest : IRequest<IEnumerable<FranchiseDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllFranchisesHandler(IGetAllRepository<FranchiseDto> franchiseRepository)
        : IRequestHandler<GetAllFranchisesRequest, IEnumerable<FranchiseDto>>
    {
        public async Task<IEnumerable<FranchiseDto>> Handle(GetAllFranchisesRequest request, CancellationToken cancellationToken)
        {
            return await franchiseRepository.GetAllAsync(cancellationToken);
        }
    }
}