using System.Collections.Immutable;
using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingCareersRequest : IRequest<List<PlayerCareerBattingDto>>
{
    public GetTopBattingCareersRequest(GetBattingCareersFilters filters)
    {
        Filters = filters;
    }

    private GetBattingCareersFilters Filters { get; }

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
    internal class GetTopBattingCareersHandler : IRequestHandler<GetTopBattingCareersRequest, List<PlayerCareerBattingDto>>
    {
        private readonly IPositionPlayerCareerRepository _positionPlayerCareerRepository;

        public GetTopBattingCareersHandler(IPositionPlayerCareerRepository positionPlayerCareerRepository)
        {
            _positionPlayerCareerRepository = positionPlayerCareerRepository;
        }

        public async Task<List<PlayerCareerBattingDto>> Handle(GetTopBattingCareersRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Filters.OrderBy is not null && !ValidOrderByProperties.Contains(request.Filters.OrderBy))
                throw new ArgumentException($"Invalid property name '{request.Filters.OrderBy}' for ordering");

            return await _positionPlayerCareerRepository.GetBattingCareers(
                request.Filters,
                cancellationToken: cancellationToken);
        }
    }
}