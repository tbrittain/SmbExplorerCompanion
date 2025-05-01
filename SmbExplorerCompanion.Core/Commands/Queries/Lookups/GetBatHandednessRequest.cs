using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetBatHandednessRequest : IRequest<List<BatHandednessDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetBatHandednessRequestHandler(IGetAllRepository<BatHandednessDto> batHandednessRepository)
        : IRequestHandler<GetBatHandednessRequest, List<BatHandednessDto>>
    {
        public async Task<List<BatHandednessDto>> Handle(GetBatHandednessRequest request, CancellationToken cancellationToken)
        {
            var batHandednessResult = await batHandednessRepository.GetAllAsync(cancellationToken);
            return batHandednessResult.ToList();
        }
    }
}