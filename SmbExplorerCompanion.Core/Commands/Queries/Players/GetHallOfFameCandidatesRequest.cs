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
        private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;
        private readonly IPitcherCareerRepository _pitcherCareerRepository;

        public GetHallOfFameCandidatesHandler(IPositionPlayerCareerRepository positionPlayerCareerRepository,
            IPitcherCareerRepository pitcherCareerRepository)
        {
            _positionPlayerCareerRepository = positionPlayerCareerRepository;
            _pitcherCareerRepository = pitcherCareerRepository;
        }

        public async Task<OneOf<RetiredPlayerCareerStatsDto, None, Exception>> Handle(GetHallOfFameCandidatesRequest request,
            CancellationToken cancellationToken)
        {
            var positionPlayersResponse = await _positionPlayerCareerRepository.GetHallOfFameCandidates(request.SeasonId, cancellationToken);
            if (positionPlayersResponse.TryPickT2(out var exception, out var rest))
            {
                return exception;
            }

            if (rest.TryPickT1(out var none, out var positionPlayers))
            {
                return none;
            }

            var pitchersResponse = await _pitcherCareerRepository.GetHallOfFameCandidates(request.SeasonId, cancellationToken);
            if (pitchersResponse.TryPickT2(out exception, out var rest2))
            {
                return exception;
            }

            if (rest2.TryPickT1(out none, out var pitchers))
            {
                return none;
            }

            return new RetiredPlayerCareerStatsDto
            {
                BattingCareers = positionPlayers,
                PitchingCareers = pitchers
            };
        }
    }
}