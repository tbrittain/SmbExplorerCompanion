using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetSimilarPlayersRequest : IRequest<List<SimilarPlayerDto>>
{
    public GetSimilarPlayersRequest(int playerId, bool isPositionPlayer)
    {
        PlayerId = playerId;
        IsPositionPlayer = isPositionPlayer;
    }

    private int PlayerId { get; }
    private bool IsPositionPlayer { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetSimilarPlayersHandler : IRequestHandler<GetSimilarPlayersRequest, List<SimilarPlayerDto>>
    {
        private readonly IPitcherCareerRepository _pitcherCareerRepository;
        private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;

        public GetSimilarPlayersHandler(IPositionPlayerCareerRepository positionPlayerCareerRepository,
            IPitcherCareerRepository pitcherCareerRepository)
        {
            _positionPlayerCareerRepository = positionPlayerCareerRepository;
            _pitcherCareerRepository = pitcherCareerRepository;
        }

        public async Task<List<SimilarPlayerDto>> Handle(GetSimilarPlayersRequest request, CancellationToken cancellationToken)
        {
            if (request.IsPositionPlayer)
                return await _positionPlayerCareerRepository.GetSimilarBattingCareers(request.PlayerId, cancellationToken);

            return await _pitcherCareerRepository.GetSimilarPitchingCareers(request.PlayerId, cancellationToken);
        }
    }
}