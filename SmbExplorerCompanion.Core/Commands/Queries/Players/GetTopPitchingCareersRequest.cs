using System.Collections.Immutable;
using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingCareersRequest : IRequest<List<PlayerCareerPitchingDto>>
{
    public GetTopPitchingCareersRequest(
        SeasonRange? seasonRange = null,
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        bool onlyHallOfFamers = false,
        int? pitcherRoleId = null)
    {
        SeasonRange = seasonRange;
        PageNumber = pageNumber;
        Limit = limit;
        OrderBy = orderBy;
        Descending = descending;
        OnlyHallOfFamers = onlyHallOfFamers;
        PitcherRoleId = pitcherRoleId;
    }

    private int? PageNumber { get; }
    private int? Limit { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }
    private bool OnlyHallOfFamers { get; }
    private int? PitcherRoleId { get; }
    private SeasonRange? SeasonRange { get; }

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
            if (request.OrderBy is not null && !ValidOrderByProperties.Contains(request.OrderBy))
                throw new ArgumentException($"Invalid property name '{request.OrderBy}' for ordering");

            return await _pitcherCareerRepository.GetPitchingCareers(
                pageNumber: request.PageNumber,
                limit: request.Limit,
                orderBy: request.OrderBy,
                descending: request.Descending,
                onlyHallOfFamers: request.OnlyHallOfFamers,
                pitcherRoleId: request.PitcherRoleId,
                seasons: request.SeasonRange,
                cancellationToken: cancellationToken);
        }
    }
}