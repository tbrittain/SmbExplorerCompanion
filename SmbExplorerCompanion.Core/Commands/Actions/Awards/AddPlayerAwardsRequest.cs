using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Commands.Actions.Awards;

public class AddPlayerAwardsRequest : IRequest<OneOf<Success, Exception>>
{
    public AddPlayerAwardsRequest(List<PlayerAwardRequestDto> playerAwardRequestDtos, int seasonId)
    {
        PlayerAwardRequestDtos = playerAwardRequestDtos;
        SeasonId = seasonId;
    }
    
    private int SeasonId { get; }

    private List<PlayerAwardRequestDto> PlayerAwardRequestDtos { get; }
    
    // ReSharper disable once UnusedType.Global
    public class AddPlayerAwardsHandler : IRequestHandler<AddPlayerAwardsRequest, OneOf<Success, Exception>>
    {
        private readonly IAwardDelegationRepository _awardDelegationRepository;

        public AddPlayerAwardsHandler(IAwardDelegationRepository awardDelegationRepository)
        {
            _awardDelegationRepository = awardDelegationRepository;
        }

        public async Task<OneOf<Success, Exception>> Handle(AddPlayerAwardsRequest request, CancellationToken cancellationToken)
        {
            return await _awardDelegationRepository.AddPlayerAwards(request.SeasonId, request.PlayerAwardRequestDtos, cancellationToken);
        }
    }
}