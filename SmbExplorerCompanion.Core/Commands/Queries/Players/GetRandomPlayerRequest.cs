using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetRandomPlayerRequest : IRequest<OneOf<PlayerBaseDto, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetRandomPlayerHandler : IRequestHandler<GetRandomPlayerRequest, OneOf<PlayerBaseDto, Exception>>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetRandomPlayerHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<OneOf<PlayerBaseDto, Exception>> Handle(GetRandomPlayerRequest request, CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetRandomPlayer();
    }
}