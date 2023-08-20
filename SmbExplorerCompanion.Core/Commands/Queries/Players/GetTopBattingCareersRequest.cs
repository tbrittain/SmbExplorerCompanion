using System.Collections.Immutable;
using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingCareersRequest : IRequest<OneOf<List<PlayerCareerBattingDto>, Exception>>
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
        nameof(PlayerCareerBattingDto.TotalSalary),
        nameof(PlayerCareerBattingDto.NumSeasons),
        nameof(PlayerCareerBattingDto.AtBats),
        nameof(PlayerCareerBattingDto.Hits),
        nameof(PlayerCareerBattingDto.Singles),
        nameof(PlayerCareerBattingDto.Doubles),
        nameof(PlayerCareerBattingDto.Triples),
        nameof(PlayerCareerBattingDto.HomeRuns),
        nameof(PlayerCareerBattingDto.Walks),
        nameof(PlayerCareerBattingDto.Runs),
        nameof(PlayerCareerBattingDto.RunsBattedIn),
        nameof(PlayerCareerBattingDto.StolenBases),
        nameof(PlayerCareerBattingDto.SacrificeHits),
        nameof(PlayerCareerBattingDto.SacrificeFlies),
        nameof(PlayerCareerBattingDto.HitByPitch),
        nameof(PlayerCareerBattingDto.Errors),
        nameof(PlayerCareerBattingDto.Strikeouts),
        nameof(PlayerCareerBattingDto.WeightedOpsPlusOrEraMinus)
    );

    // ReSharper disable once UnusedType.Global
    internal class GetTopBattingCareersHandler : IRequestHandler<GetTopBattingCareersRequest, OneOf<List<PlayerCareerBattingDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopBattingCareersHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerCareerBattingDto>, Exception>> Handle(GetTopBattingCareersRequest request, CancellationToken cancellationToken)
        {
            if (request.OrderBy is not null && !ValidOrderByProperties.Contains(request.OrderBy))
                return new ArgumentException($"Invalid property name '{request.OrderBy}' for ordering");

            return await _playerRepository.GetBattingCareers(
                pageNumber: request.PageNumber, 
                orderBy: request.OrderBy, 
                descending: request.Descending, 
                cancellationToken: cancellationToken);
        }
    }
}