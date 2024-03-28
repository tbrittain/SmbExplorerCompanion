using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerGameStatPercentilesRequest : IRequest<PlayerGameStatPercentileDto>
{
    public GetPlayerGameStatPercentilesRequest(int playerId, int seasonId, bool isPitcher, int? pitcherRoleId)
    {
        PlayerId = playerId;
        SeasonId = seasonId;
        IsPitcher = isPitcher;
        PitcherRoleId = pitcherRoleId;
    }

    private int PlayerId { get; }
    private int SeasonId { get; }
    private bool IsPitcher { get; }
    private int? PitcherRoleId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetPlayerGameStatPercentilesHandler : IRequestHandler<GetPlayerGameStatPercentilesRequest,
        PlayerGameStatPercentileDto>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetPlayerGameStatPercentilesHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<PlayerGameStatPercentileDto> Handle(GetPlayerGameStatPercentilesRequest request,
            CancellationToken cancellationToken)
        {
            return await _generalPlayerRepository.GetPlayerGameStatPercentiles(request.PlayerId,
                request.SeasonId,
                request.IsPitcher,
                request.PitcherRoleId,
                cancellationToken);
        }
    }
}