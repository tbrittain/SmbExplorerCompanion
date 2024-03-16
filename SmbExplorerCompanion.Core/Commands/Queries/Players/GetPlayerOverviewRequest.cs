using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerOverviewRequest : IRequest<PlayerOverviewDto>
{
    public GetPlayerOverviewRequest(int playerId)
    {
        PlayerId = playerId;
    }

    private int PlayerId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetPlayerOverviewHandler : IRequestHandler<GetPlayerOverviewRequest, PlayerOverviewDto>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetPlayerOverviewHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<PlayerOverviewDto> Handle(GetPlayerOverviewRequest request, CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetHistoricalPlayer(request.PlayerId, cancellationToken);
    }
}