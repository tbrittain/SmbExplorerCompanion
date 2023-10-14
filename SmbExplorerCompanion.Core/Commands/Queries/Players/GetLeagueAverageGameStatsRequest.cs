﻿using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetLeagueAverageGameStatsRequest : IRequest<OneOf<GameStatDto, Exception>>
{
    public GetLeagueAverageGameStatsRequest(int seasonId, bool isPitcher)
    {
        SeasonId = seasonId;
        IsPitcher = isPitcher;
    }

    private int SeasonId { get; }
    private bool IsPitcher { get; }
    
    // ReSharper disable once UnusedType.Global
    internal class GetLeagueAverageGameStatsHandler : IRequestHandler<GetLeagueAverageGameStatsRequest, OneOf<GameStatDto, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetLeagueAverageGameStatsHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<GameStatDto, Exception>> Handle(GetLeagueAverageGameStatsRequest request, CancellationToken cancellationToken) =>
            await _playerRepository.GetLeagueAverageGameStats(request.SeasonId, request.IsPitcher, cancellationToken);
    }
}