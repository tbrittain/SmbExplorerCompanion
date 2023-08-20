using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Commands.Actions.Awards;

public class AddHallOfFamersRequest : IRequest<OneOf<Success, Exception>>
{
    public AddHallOfFamersRequest(List<PlayerHallOfFameRequestDto> players)
    {
        Players = players;
    }

    private List<PlayerHallOfFameRequestDto> Players { get; }

    // ReSharper disable once UnusedType.Global
    internal class AddHallOfFamersHandler : IRequestHandler<AddHallOfFamersRequest, OneOf<Success, Exception>>
    {
        private readonly IAwardDelegationRepository _awardDelegationRepository;

        public AddHallOfFamersHandler(IAwardDelegationRepository awardDelegationRepository)
        {
            _awardDelegationRepository = awardDelegationRepository;
        }

        public async Task<OneOf<Success, Exception>> Handle(AddHallOfFamersRequest request, CancellationToken cancellationToken)
        {
            return await _awardDelegationRepository.AddHallOfFameAwards(request.Players, cancellationToken);
        }
    }
}