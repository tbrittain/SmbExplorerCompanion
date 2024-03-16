using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetRandomPlayerRequest : IRequest<PlayerBaseDto>
{
    // ReSharper disable once UnusedType.Global
    internal class GetRandomPlayerHandler : IRequestHandler<GetRandomPlayerRequest, PlayerBaseDto>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetRandomPlayerHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<PlayerBaseDto> Handle(GetRandomPlayerRequest request, CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetRandomPlayer();
    }
}