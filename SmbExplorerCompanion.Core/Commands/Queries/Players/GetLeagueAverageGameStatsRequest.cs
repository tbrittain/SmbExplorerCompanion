﻿using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetLeagueAverageGameStatsRequest : IRequest<OneOf<GameStatDto, Exception>>
{
    public GetLeagueAverageGameStatsRequest(int seasonId, bool isPitcher, int? pitcherRoleId)
    {
        SeasonId = seasonId;
        IsPitcher = isPitcher;
        PitcherRoleId = pitcherRoleId;
    }

    private int SeasonId { get; }
    private bool IsPitcher { get; }
    private int? PitcherRoleId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetLeagueAverageGameStatsHandler : IRequestHandler<GetLeagueAverageGameStatsRequest, OneOf<GameStatDto, Exception>>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetLeagueAverageGameStatsHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<OneOf<GameStatDto, Exception>> Handle(GetLeagueAverageGameStatsRequest request, CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetLeagueAverageGameStats(request.SeasonId,
                request.IsPitcher,
                request.PitcherRoleId,
                cancellationToken);
    }
}