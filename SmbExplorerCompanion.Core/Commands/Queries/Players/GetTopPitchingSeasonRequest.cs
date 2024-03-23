using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingSeasonRequest : IRequest<List<PlayerPitchingSeasonDto>>
{
    public GetTopPitchingSeasonRequest(GetPitchingSeasonsFilters filters)
    {
        Filters = filters;
    }
    
    private GetPitchingSeasonsFilters Filters { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopPitchingSeasonHandler : IRequestHandler<GetTopPitchingSeasonRequest, List<PlayerPitchingSeasonDto>>
    {
        private readonly IPitcherSeasonRepository _pitcherSeasonRepository;

        public GetTopPitchingSeasonHandler(IPitcherSeasonRepository pitcherSeasonRepository)
        {
            _pitcherSeasonRepository = pitcherSeasonRepository;
        }

        public async Task<List<PlayerPitchingSeasonDto>> Handle(GetTopPitchingSeasonRequest request,
            CancellationToken cancellationToken) =>
            await _pitcherSeasonRepository.GetPitchingSeasons(
                filters: request.Filters,
                cancellationToken: cancellationToken);
    }
}