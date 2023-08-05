using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetAllPositionsRequest : IRequest<OneOf<List<PositionDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    public class GetAllPositionsHandler : IRequestHandler<GetAllPositionsRequest, OneOf<List<PositionDto>, Exception>>
    {
        private readonly IRepository<PositionDto> _positionRepository;

        public GetAllPositionsHandler(IRepository<PositionDto> positionRepository)
        {
            _positionRepository = positionRepository;
        }

        public async Task<OneOf<List<PositionDto>, Exception>> Handle(GetAllPositionsRequest request, CancellationToken cancellationToken)
        {
            var positionResult = await _positionRepository.GetAllAsync(cancellationToken);
            if (positionResult.TryPickT1(out var exception, out var positionDtos))
            {
                return exception;
            }

            return positionDtos.ToList();
        }
    }
}