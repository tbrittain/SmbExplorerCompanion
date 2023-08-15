﻿using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingSeasonRequest : IRequest<OneOf<List<PlayerPitchingSeasonDto>, Exception>>
{
    public GetTopPitchingSeasonRequest(
        int? seasonId = null,
        int? limit = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        bool descending = true,
        int? teamId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false)
    {
        OnlyUserAssignableAwards = onlyUserAssignableAwards;
        SeasonId = seasonId;
        Limit = limit;
        IsPlayoffs = isPlayoffs;
        PageNumber = pageNumber;
        OrderBy = orderBy;
        Descending = descending;
        OnlyRookies = onlyRookies;
        TeamId = teamId;
        IncludeChampionAwards = includeChampionAwards;
    }

    private int? SeasonId { get; }
    private bool IsPlayoffs { get; }
    private int? PageNumber { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }
    private int? Limit { get; }
    private int? TeamId { get; }
    private bool OnlyRookies { get; }
    private bool IncludeChampionAwards { get; }
    private bool OnlyUserAssignableAwards { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopPitchingSeasonHandler : IRequestHandler<GetTopPitchingSeasonRequest, OneOf<List<PlayerPitchingSeasonDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopPitchingSeasonHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> Handle(GetTopPitchingSeasonRequest request,
            CancellationToken cancellationToken)
        {
            return await _playerRepository.GetTopPitchingSeasons(request.SeasonId,
                request.IsPlayoffs,
                request.PageNumber,
                request.OrderBy,
                request.Limit,
                request.Descending,
                request.TeamId,
                request.OnlyRookies,
                request.IncludeChampionAwards,
                request.OnlyUserAssignableAwards,
                cancellationToken);
        }
    }
}