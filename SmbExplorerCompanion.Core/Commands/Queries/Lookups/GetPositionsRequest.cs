using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPositionsRequest : IRequest<List<PositionDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPositionsHandler(IGetAllRepository<PositionDto> positionRepository)
        : IRequestHandler<GetPositionsRequest, List<PositionDto>>
    {
        public async Task<List<PositionDto>> Handle(GetPositionsRequest request, CancellationToken cancellationToken)
        {
            var positionResult = await positionRepository.GetAllAsync(cancellationToken);
            return positionResult.ToList();
        }
    }
}