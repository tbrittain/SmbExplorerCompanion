using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingSeasonRequest : IRequest<List<PlayerBattingSeasonDto>>
{
    public GetTopBattingSeasonRequest(
        SeasonRange? seasons = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        bool descending = true,
        int? limit = null,
        int? teamId = null,
        int? primaryPositionId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false)
    {
        OnlyUserAssignableAwards = onlyUserAssignableAwards;
        Seasons = seasons;
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

    private SeasonRange? Seasons { get; }
    private bool IsPlayoffs { get; }
    private int? PageNumber { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }
    private int? Limit { get; }
    private int? TeamId { get; }
    private int? PrimaryPositionId { get; }
    private bool OnlyRookies { get; }
    private bool IncludeChampionAwards { get; }
    private bool OnlyUserAssignableAwards { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopBattingSeasonHandler : IRequestHandler<GetTopBattingSeasonRequest, List<PlayerBattingSeasonDto>>
    {
        private readonly IPositionPlayerSeasonRepository _positionPlayerSeasonRepository;

        public GetTopBattingSeasonHandler(IPositionPlayerSeasonRepository positionPlayerSeasonRepository)
        {
            _positionPlayerSeasonRepository = positionPlayerSeasonRepository;
        }

        public async Task<List<PlayerBattingSeasonDto>> Handle(GetTopBattingSeasonRequest request,
            CancellationToken cancellationToken) =>
            await _positionPlayerSeasonRepository.GetBattingSeasons(
                seasons: request.Seasons,
                isPlayoffs: request.IsPlayoffs,
                pageNumber: request.PageNumber,
                orderBy: request.OrderBy,
                limit: request.Limit,
                descending: request.Descending,
                teamId: request.TeamId,
                primaryPositionId: request.PrimaryPositionId,
                onlyRookies: request.OnlyRookies,
                includeChampionAwards: request.IncludeChampionAwards,
                onlyUserAssignableAwards: request.OnlyUserAssignableAwards,
                cancellationToken: cancellationToken);
    }
}