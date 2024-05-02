using MediatR;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Commands.Actions.Awards;

public class AddHallOfFamersRequest : IRequest
{
    public AddHallOfFamersRequest(List<PlayerHallOfFameRequestDto> players)
    {
        Players = players;
    }

    private List<PlayerHallOfFameRequestDto> Players { get; }

    // ReSharper disable once UnusedType.Global
    internal class AddHallOfFamersHandler : IRequestHandler<AddHallOfFamersRequest>
    {
        private readonly IAwardDelegationRepository _awardDelegationRepository;

        public AddHallOfFamersHandler(IAwardDelegationRepository awardDelegationRepository)
        {
            _awardDelegationRepository = awardDelegationRepository;
        }

        public Task Handle(AddHallOfFamersRequest request, CancellationToken cancellationToken)
        {
            return _awardDelegationRepository.AddHallOfFameAwards(request.Players, cancellationToken);
        }
    }
}