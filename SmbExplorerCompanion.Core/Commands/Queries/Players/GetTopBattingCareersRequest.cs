using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingCareersRequest : IRequest<List<PlayerCareerBattingDto>>
{
    public GetTopBattingCareersRequest(GetBattingCareersFilters filters)
    {
        Filters = filters;
    }

    private GetBattingCareersFilters Filters { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopBattingCareersHandler : IRequestHandler<GetTopBattingCareersRequest, List<PlayerCareerBattingDto>>
    {
        private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;

        public GetTopBattingCareersHandler(IPositionPlayerCareerRepository positionPlayerCareerRepository)
        {
            _positionPlayerCareerRepository = positionPlayerCareerRepository;
        }

        public async Task<List<PlayerCareerBattingDto>> Handle(GetTopBattingCareersRequest request,
            CancellationToken cancellationToken)
        {
            return await _positionPlayerCareerRepository.GetBattingCareers(
                request.Filters,
                cancellationToken);
        }
    }
}