using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerKpiPercentilesRequest : IRequest<OneOf<PlayerKpiPercentileDto, Exception>>
{
    public GetPlayerKpiPercentilesRequest(int playerId, int seasonId, bool isPitcher, int? pitcherRoleId)
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
    internal class GetPlayerKpiPercentilesHandler : IRequestHandler<GetPlayerKpiPercentilesRequest,
        OneOf<PlayerKpiPercentileDto, Exception>>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetPlayerKpiPercentilesHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<OneOf<PlayerKpiPercentileDto, Exception>> Handle(GetPlayerKpiPercentilesRequest request,
            CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetPlayerKpiPercentiles(request.PlayerId,
                request.SeasonId,
                request.IsPitcher,
                request.PitcherRoleId,
                cancellationToken);
    }
}