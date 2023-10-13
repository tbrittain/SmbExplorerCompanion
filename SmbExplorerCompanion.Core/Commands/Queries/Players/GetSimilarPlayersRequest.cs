using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetSimilarPlayersRequest : IRequest<OneOf<List<SimilarPlayerDto>, Exception>>
{
    public GetSimilarPlayersRequest(int playerId, bool isPositionPlayer)
    {
        PlayerId = playerId;
        IsPositionPlayer = isPositionPlayer;
    }

    private int PlayerId { get; }
    private bool IsPositionPlayer { get; set; }
    
    // ReSharper disable once UnusedType.Global
    internal class GetSimilarPlayersHandler : IRequestHandler<GetSimilarPlayersRequest, OneOf<List<SimilarPlayerDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetSimilarPlayersHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<SimilarPlayerDto>, Exception>> Handle(GetSimilarPlayersRequest request, CancellationToken cancellationToken)
        {
            if (request.IsPositionPlayer)
                return await _playerRepository.GetSimilarBattingCareers(request.PlayerId, cancellationToken);
            return await _playerRepository.GetSimilarPitchingCareers(request.PlayerId, cancellationToken);
        }
    }
}