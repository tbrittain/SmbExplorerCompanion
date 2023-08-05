using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Commands.Actions.Awards;

public class AddPlayerAwardsRequest : IRequest<OneOf<Success, Exception>>
{
    public AddPlayerAwardsRequest(List<PlayerAwardRequestDto> playerAwardRequestDtos)
    {
        PlayerAwardRequestDtos = playerAwardRequestDtos;
    }

    private List<PlayerAwardRequestDto> PlayerAwardRequestDtos { get; }
    
    // ReSharper disable once UnusedType.Global
    public class AddPlayerAwardsHandler : IRequestHandler<AddPlayerAwardsRequest, OneOf<Success, Exception>>
    {
        private readonly IAwardRepository _awardRepository;

        public AddPlayerAwardsHandler(IAwardRepository awardRepository)
        {
            _awardRepository = awardRepository;
        }

        public async Task<OneOf<Success, Exception>> Handle(AddPlayerAwardsRequest request, CancellationToken cancellationToken)
        {
            return await _awardRepository.AddPlayerAwards(request.PlayerAwardRequestDtos, cancellationToken);
        }
    }
}