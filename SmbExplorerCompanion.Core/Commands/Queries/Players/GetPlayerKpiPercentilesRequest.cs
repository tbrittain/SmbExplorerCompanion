﻿using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetPlayerKpiPercentilesRequest : IRequest<PlayerKpiPercentileDto>
{
    public GetPlayerKpiPercentilesRequest(int playerId, int seasonId, bool isPitcher, int? pitcherRoleId = null)
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
        PlayerKpiPercentileDto>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetPlayerKpiPercentilesHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<PlayerKpiPercentileDto> Handle(GetPlayerKpiPercentilesRequest request,
            CancellationToken cancellationToken)
        {
            return await _generalPlayerRepository.GetPlayerKpiPercentiles(request.PlayerId,
                request.SeasonId,
                request.IsPitcher,
                request.PitcherRoleId,
                cancellationToken);
        }
    }
}