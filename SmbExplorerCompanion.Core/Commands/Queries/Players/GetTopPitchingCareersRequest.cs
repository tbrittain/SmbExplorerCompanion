using System.Collections.Immutable;
using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingCareersRequest : IRequest<OneOf<List<PlayerCareerDto>, Exception>>
{
    public GetTopPitchingCareersRequest(int? pageNumber = null, string? orderBy = null, bool descending = true)
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
        nameof(PlayerCareerDto.NumSeasons),
        nameof(PlayerCareerDto.Wins),
        nameof(PlayerCareerDto.Losses),
        nameof(PlayerCareerDto.GamesStarted),
        nameof(PlayerCareerDto.Saves),
        nameof(PlayerCareerDto.InningsPitched),
        nameof(PlayerCareerDto.Hits),
        nameof(PlayerCareerDto.CompleteGames),
        nameof(PlayerCareerDto.Shutouts),
        nameof(PlayerCareerDto.HomeRuns),
        nameof(PlayerCareerDto.Walks),
        nameof(PlayerCareerDto.Strikeouts),
        nameof(PlayerCareerDto.EarnedRuns),
        nameof(PlayerCareerDto.WeightedOpsPlusOrEraMinus)
    );

    // ReSharper disable once UnusedType.Global
    internal class GetTopPitchingCareersHandler : IRequestHandler<GetTopPitchingCareersRequest, OneOf<List<PlayerCareerDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopPitchingCareersHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerCareerDto>, Exception>> Handle(GetTopPitchingCareersRequest request, CancellationToken cancellationToken)
        {
            if (request.OrderBy is not null && !ValidOrderByProperties.Contains(request.OrderBy))
                return new ArgumentException($"Invalid property name '{request.OrderBy}' for ordering");

            return await _playerRepository.GetTopPitchingCareers(request.PageNumber, request.OrderBy, request.Descending, cancellationToken);
        }
    }
}