using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingSeasonRequest : IRequest<List<PlayerBattingSeasonDto>>
{
    public GetTopBattingSeasonRequest(GetBattingSeasonsFilters filters)
    {
        Filters = filters;
    }

    private GetBattingSeasonsFilters Filters { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopBattingSeasonHandler : IRequestHandler<GetTopBattingSeasonRequest, List<PlayerBattingSeasonDto>>
    {
        private readonly IPositionPlayerSeasonRepository _positionPlayerSeasonRepository;

        public GetTopBattingSeasonHandler(IPositionPlayerSeasonRepository positionPlayerSeasonRepository)
        {
            _positionPlayerSeasonRepository = positionPlayerSeasonRepository;
        }

        public async Task<List<PlayerBattingSeasonDto>> Handle(GetTopBattingSeasonRequest request,
            CancellationToken cancellationToken)
        {
            return await _positionPlayerSeasonRepository.GetBattingSeasons(
                request.Filters,
                cancellationToken);
        }
    }
}