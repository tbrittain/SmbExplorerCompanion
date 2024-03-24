using System.Collections.Immutable;
using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingCareersRequest : IRequest<List<PlayerCareerPitchingDto>>
{
    public GetTopPitchingCareersRequest(GetPitchingCareersFilters filters)
    {
        Filters = filters;
    }

    private GetPitchingCareersFilters Filters { get; }

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
        nameof(PlayerCareerPitchingDto.TotalPitches),
        nameof(PlayerCareerPitchingDto.WeightedOpsPlusOrEraMinus)
    );

    // ReSharper disable once UnusedType.Global
    internal class GetTopPitchingCareersHandler : IRequestHandler<GetTopPitchingCareersRequest, List<PlayerCareerPitchingDto>>
    {
        private readonly IPitcherCareerRepository _pitcherCareerRepository;

        public GetTopPitchingCareersHandler(IPitcherCareerRepository pitcherCareerRepository)
        {
            _pitcherCareerRepository = pitcherCareerRepository;
        }

        public async Task<List<PlayerCareerPitchingDto>> Handle(GetTopPitchingCareersRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Filters.OrderBy is not null && !ValidOrderByProperties.Contains(request.Filters.OrderBy))
                throw new ArgumentException($"Invalid property name '{request.Filters.OrderBy}' for ordering");

            return await _pitcherCareerRepository.GetPitchingCareers(
                request.Filters,
                cancellationToken);
        }
    }
}