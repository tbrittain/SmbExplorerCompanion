using System.Collections.Immutable;
using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingCareersRequest : IRequest<OneOf<List<PlayerCareerPitchingDto>, Exception>>
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
        
        nameof(PlayerCareerPitchingDto.TotalSalary),
        nameof(PlayerCareerPitchingDto.NumSeasons),
        nameof(PlayerCareerPitchingDto.Wins),
        nameof(PlayerCareerPitchingDto.Losses),
        nameof(PlayerCareerPitchingDto.GamesStarted),
        nameof(PlayerCareerPitchingDto.Saves),
        nameof(PlayerCareerPitchingDto.InningsPitched),
        nameof(PlayerCareerPitchingDto.Hits),
        nameof(PlayerCareerPitchingDto.CompleteGames),
        nameof(PlayerCareerPitchingDto.Shutouts),
        nameof(PlayerCareerPitchingDto.HomeRuns),
        nameof(PlayerCareerPitchingDto.Walks),
        nameof(PlayerCareerPitchingDto.Strikeouts),
        nameof(PlayerCareerPitchingDto.EarnedRuns),
        nameof(PlayerCareerPitchingDto.WeightedOpsPlusOrEraMinus)
    );

    // ReSharper disable once UnusedType.Global
    internal class GetTopPitchingCareersHandler : IRequestHandler<GetTopPitchingCareersRequest, OneOf<List<PlayerCareerPitchingDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopPitchingCareersHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerCareerPitchingDto>, Exception>> Handle(GetTopPitchingCareersRequest request, CancellationToken cancellationToken)
        {
            if (request.OrderBy is not null && !ValidOrderByProperties.Contains(request.OrderBy))
                return new ArgumentException($"Invalid property name '{request.OrderBy}' for ordering");

            return await _playerRepository.GetPitchingCareers(
                pageNumber: request.PageNumber, 
                orderBy: request.OrderBy, 
                descending: request.Descending, 
                cancellationToken: cancellationToken);
        }
    }
}