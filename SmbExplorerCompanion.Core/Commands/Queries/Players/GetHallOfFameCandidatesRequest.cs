using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetHallOfFameCandidatesRequest : IRequest<RetiredPlayerCareerStatsDto>
{
    public GetHallOfFameCandidatesRequest(int seasonId)
    {
        SeasonId = seasonId;
    }

    private int SeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetHallOfFameCandidatesHandler : IRequestHandler<GetHallOfFameCandidatesRequest, RetiredPlayerCareerStatsDto>
    {
        private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;
        private readonly IPitcherCareerRepository _pitcherCareerRepository;

        public GetHallOfFameCandidatesHandler(IPositionPlayerCareerRepository positionPlayerCareerRepository,
            IPitcherCareerRepository pitcherCareerRepository)
        {
            _positionPlayerCareerRepository = positionPlayerCareerRepository;
            _pitcherCareerRepository = pitcherCareerRepository;
        }

        public async Task<RetiredPlayerCareerStatsDto> Handle(GetHallOfFameCandidatesRequest request,
            CancellationToken cancellationToken)
        {
            var positionPlayersResponse = await _positionPlayerCareerRepository.GetHallOfFameCandidates(request.SeasonId, cancellationToken);

            var pitchersResponse = await _pitcherCareerRepository.GetHallOfFameCandidates(request.SeasonId, cancellationToken);

            return new RetiredPlayerCareerStatsDto
            {
                BattingCareers = positionPlayersResponse,
                PitchingCareers = pitchersResponse
            };
        }
    }
}