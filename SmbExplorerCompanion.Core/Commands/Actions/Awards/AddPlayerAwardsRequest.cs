using MediatR;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Commands.Actions.Awards;

public class AddPlayerAwardsRequest : IRequest
{
    public AddPlayerAwardsRequest(List<PlayerAwardRequestDto> playerAwardRequestDtos, int seasonId)
    {
        PlayerAwardRequestDtos = playerAwardRequestDtos;
        SeasonId = seasonId;
    }

    private int SeasonId { get; }

    private List<PlayerAwardRequestDto> PlayerAwardRequestDtos { get; }

    // ReSharper disable once UnusedType.Global
    internal class AddPlayerAwardsHandler : IRequestHandler<AddPlayerAwardsRequest>
    {
        private readonly IAwardDelegationRepository _awardDelegationRepository;

        public AddPlayerAwardsHandler(IAwardDelegationRepository awardDelegationRepository)
        {
            _awardDelegationRepository = awardDelegationRepository;
        }

        public Task Handle(AddPlayerAwardsRequest request, CancellationToken cancellationToken)
        {
            return _awardDelegationRepository.AddRegularSeasonPlayerAwards(request.SeasonId, request.PlayerAwardRequestDtos, cancellationToken);
        }
    }
}