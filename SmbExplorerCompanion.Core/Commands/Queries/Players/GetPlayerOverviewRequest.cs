using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerOverviewRequest : IRequest<OneOf<PlayerOverviewDto, Exception>>
{
    public GetPlayerOverviewRequest(int playerId)
    {
        PlayerId = playerId;
    }

    private int PlayerId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetPlayerOverviewHandler : IRequestHandler<GetPlayerOverviewRequest, OneOf<PlayerOverviewDto, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetPlayerOverviewHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<PlayerOverviewDto, Exception>> Handle(GetPlayerOverviewRequest request, CancellationToken cancellationToken) =>
            await _playerRepository.GetHistoricalPlayer(request.PlayerId, cancellationToken);
    }
}