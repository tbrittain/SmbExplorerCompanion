using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerFieldingRankingsRequest : IRequest<OneOf<List<PlayerFieldingRankingDto>, Exception>>
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
        OneOf<List<PlayerFieldingRankingDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetPlayerFieldingRankingsHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerFieldingRankingDto>, Exception>> Handle(GetPlayerFieldingRankingsRequest request,
            CancellationToken cancellationToken) =>
            await _playerRepository.GetPlayerFieldingRankings(request.SeasonId,
                request.PrimaryPositionId,
                request.PageNumber,
                request.Limit,
                cancellationToken);
    }
}