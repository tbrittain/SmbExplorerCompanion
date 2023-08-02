using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingSeasonRequest : IRequest<OneOf<List<PlayerPitchingSeasonDto>, Exception>>
{
    public GetTopPitchingSeasonRequest(int seasonId, bool isPlayoffs, int? pageNumber, string? orderBy, bool descending)
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
    public class GetTopPitchingSeasonHandler : IRequestHandler<GetTopPitchingSeasonRequest, OneOf<List<PlayerPitchingSeasonDto>, Exception>>
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
                request.Descending,
                null,
                cancellationToken);
        }
    }
}