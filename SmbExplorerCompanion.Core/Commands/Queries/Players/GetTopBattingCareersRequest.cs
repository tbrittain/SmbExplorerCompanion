using System.Collections.Immutable;
using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingCareersRequest : IRequest<List<PlayerCareerBattingDto>>
{
    public GetTopBattingCareersRequest(int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        bool onlyHallOfFamers = false,
        int? primaryPositionId = null)
    {
        PageNumber = pageNumber;
        Limit = limit;
        OrderBy = orderBy;
        Descending = descending;
        OnlyHallOfFamers = onlyHallOfFamers;
        PrimaryPositionId = primaryPositionId;
    }

    private int? PageNumber { get; }
    private int? Limit { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }
    private bool OnlyHallOfFamers { get; }
    private int? PrimaryPositionId { get; }

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
            if (request.OrderBy is not null && !ValidOrderByProperties.Contains(request.OrderBy))
                throw new ArgumentException($"Invalid property name '{request.OrderBy}' for ordering");

            return await _positionPlayerCareerRepository.GetBattingCareers(
                pageNumber: request.PageNumber,
                limit: request.Limit,
                orderBy: request.OrderBy,
                descending: request.Descending,
                onlyHallOfFamers: request.OnlyHallOfFamers,
                primaryPositionId: request.PrimaryPositionId,
                cancellationToken: cancellationToken);
        }
    }
}