using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerFieldingRankingsRequest : IRequest<List<PlayerFieldingRankingDto>>
{
    public GetPlayerFieldingRankingsRequest(int seasonId, int? primaryPositionId, int? pageNumber, int? limit)
    {
        SeasonId = seasonId;
        PrimaryPositionId = primaryPositionId;
        PageNumber = pageNumber;
        Limit = limit;
    }

    private int SeasonId { get; }
    private int? PrimaryPositionId { get; }
    private int? PageNumber { get; }
    private int? Limit { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetPlayerFieldingRankingsHandler : IRequestHandler<GetPlayerFieldingRankingsRequest,
        List<PlayerFieldingRankingDto>>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetPlayerFieldingRankingsHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<List<PlayerFieldingRankingDto>> Handle(GetPlayerFieldingRankingsRequest request,
            CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetPlayerFieldingRankings(request.SeasonId,
                request.PrimaryPositionId,
                request.PageNumber,
                request.Limit,
                cancellationToken);
    }
}