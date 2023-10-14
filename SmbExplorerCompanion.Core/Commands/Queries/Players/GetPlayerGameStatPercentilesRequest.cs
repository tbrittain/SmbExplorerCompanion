using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerGameStatPercentilesRequest : IRequest<OneOf<PlayerGameStatPercentileDto, Exception>>
{
    public GetPlayerGameStatPercentilesRequest(int playerId, int seasonId, bool isPitcher)
    {
        PlayerId = playerId;
        SeasonId = seasonId;
        IsPitcher = isPitcher;
    }

    private int PlayerId { get; }
    private int SeasonId { get; }
    private bool IsPitcher { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetPlayerGameStatPercentilesHandler : IRequestHandler<GetPlayerGameStatPercentilesRequest,
        OneOf<PlayerGameStatPercentileDto, Exception>>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetPlayerGameStatPercentilesHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<OneOf<PlayerGameStatPercentileDto, Exception>> Handle(GetPlayerGameStatPercentilesRequest request,
            CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetPlayerGameStatPercentiles(request.PlayerId, request.SeasonId, request.IsPitcher, cancellationToken);
    }
}