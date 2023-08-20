using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetHallOfFameCandidatesRequest : IRequest<OneOf<RetiredPlayerCareerStatsDto, None, Exception>>
{
    public GetHallOfFameCandidatesRequest(int seasonId)
    {
        SeasonId = seasonId;
    }

    private int SeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetHallOfFameCandidatesHandler : IRequestHandler<GetHallOfFameCandidatesRequest,
        OneOf<RetiredPlayerCareerStatsDto, None, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetHallOfFameCandidatesHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<RetiredPlayerCareerStatsDto, None, Exception>> Handle(GetHallOfFameCandidatesRequest request,
            CancellationToken cancellationToken)
        {
            return await _playerRepository.GetHallOfFameCandidates(request.SeasonId, cancellationToken);
        }
    }
}