using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingSeasonRequest : IRequest<OneOf<List<PlayerBattingSeasonDto>, Exception>>
{
    public GetTopBattingSeasonRequest(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        bool descending = true,
        int? limit = null,
        int? teamId = null,
        int? primaryPositionId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true)
    {
        SeasonId = seasonId;
        OnlyRookies = onlyRookies;
        IsPlayoffs = isPlayoffs;
        PageNumber = pageNumber;
        OrderBy = orderBy;
        Descending = descending;
        Limit = limit;
        TeamId = teamId;
        PrimaryPositionId = primaryPositionId;
        IncludeChampionAwards = includeChampionAwards;
    }

    private int? SeasonId { get; }
    private bool IsPlayoffs { get; }
    private int? PageNumber { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }
    private int? Limit { get; }
    private int? TeamId { get; }
    private int? PrimaryPositionId { get; }
    private bool OnlyRookies { get; }
    private bool IncludeChampionAwards { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopBattingSeasonHandler : IRequestHandler<GetTopBattingSeasonRequest, OneOf<List<PlayerBattingSeasonDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopBattingSeasonHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> Handle(GetTopBattingSeasonRequest request,
            CancellationToken cancellationToken)
        {
            return await _playerRepository.GetTopBattingSeasons(request.SeasonId,
                request.IsPlayoffs,
                request.PageNumber,
                request.OrderBy,
                request.Limit,
                request.Descending,
                request.TeamId,
                request.PrimaryPositionId,
                request.OnlyRookies,
                request.IncludeChampionAwards,
                cancellationToken);
        }
    }
}