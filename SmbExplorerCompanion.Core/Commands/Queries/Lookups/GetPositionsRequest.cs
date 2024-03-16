using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPositionsRequest : IRequest<List<PositionDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPositionsHandler : IRequestHandler<GetPositionsRequest, List<PositionDto>>
    {
        private readonly IRepository<PositionDto> _positionRepository;

        public GetAllPositionsHandler(IRepository<PositionDto> positionRepository)
        {
            _positionRepository = positionRepository;
        }

        public async Task<List<PositionDto>> Handle(GetPositionsRequest request, CancellationToken cancellationToken)
        {
            var positionResult = await _positionRepository.GetAllAsync(cancellationToken);
            return positionResult.ToList();
        }
    }
}