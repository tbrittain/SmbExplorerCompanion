using System.Collections.Immutable;
using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingCareersRequest : IRequest<OneOf<List<PlayerCareerDto>, Exception>>
{
    public GetTopBattingCareersRequest(int? pageNumber = null, string? orderBy = null, bool descending = true)
    {
        PageNumber = pageNumber;
        OrderBy = orderBy;
        Descending = descending;
    }

    private int? PageNumber { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }

    private static ImmutableArray<string> ValidOrderByProperties { get; } = ImmutableArray.Create(
        nameof(PlayerCareerDto.TotalSalary),
        nameof(PlayerCareerDto.AtBats),
        nameof(PlayerCareerDto.Hits),
        nameof(PlayerCareerDto.Singles),
        nameof(PlayerCareerDto.Doubles),
        nameof(PlayerCareerDto.Triples),
        nameof(PlayerCareerDto.HomeRuns),
        nameof(PlayerCareerDto.Walks),
        nameof(PlayerCareerDto.Runs),
        nameof(PlayerCareerDto.RunsBattedIn),
        nameof(PlayerCareerDto.StolenBases),
        nameof(PlayerCareerDto.SacrificeHits),
        nameof(PlayerCareerDto.SacrificeFlies),
        nameof(PlayerCareerDto.Errors),
        nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus)
    );

    // ReSharper disable once UnusedType.Global
    public class GetTopBattingCareersHandler : IRequestHandler<GetTopBattingCareersRequest, OneOf<List<PlayerCareerDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopBattingCareersHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerCareerDto>, Exception>> Handle(GetTopBattingCareersRequest request, CancellationToken cancellationToken)
        {
            if (request.OrderBy is not null && !ValidOrderByProperties.Contains(request.OrderBy))
                return new ArgumentException($"Invalid property name '{request.OrderBy}' for ordering");

            return await _playerRepository.GetTopBattingCareers(request.PageNumber, request.OrderBy, request.Descending, cancellationToken);
        }
    }
}