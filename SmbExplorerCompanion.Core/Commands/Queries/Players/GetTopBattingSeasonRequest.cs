using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingSeasonRequest : IRequest<OneOf<List<PlayerBattingSeasonDto>, Exception>>
{
    public GetTopBattingSeasonRequest(int seasonId, bool isPlayoffs, int? pageNumber, string? orderBy, bool descending)
    {
        SeasonId = seasonId;
        IsPlayoffs = isPlayoffs;
        PageNumber = pageNumber;
        OrderBy = orderBy;
        Descending = descending;
    }

    private int SeasonId { get; }
    private bool IsPlayoffs { get; }
    private int? PageNumber { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }

    // ReSharper disable once UnusedType.Global
    public class GetTopBattingSeasonHandler : IRequestHandler<GetTopBattingSeasonRequest, OneOf<List<PlayerBattingSeasonDto>, Exception>>
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
                request.Descending,
                cancellationToken);
        }
    }
}