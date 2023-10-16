using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetTeamScheduleBreakdownRequest : IRequest<OneOf<DivisionScheduleBreakdownDto, Exception>>
{
    public GetTeamScheduleBreakdownRequest(int teamSeasonId, bool? includeDivision = null)
    {
        TeamSeasonId = teamSeasonId;
        IncludeDivision = includeDivision ?? true;
    }

    private int TeamSeasonId { get; }
    private bool IncludeDivision { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTeamScheduleBreakdownHandler : IRequestHandler<GetTeamScheduleBreakdownRequest,
        OneOf<DivisionScheduleBreakdownDto, Exception>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetTeamScheduleBreakdownHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<OneOf<DivisionScheduleBreakdownDto, Exception>> Handle(GetTeamScheduleBreakdownRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetTeamScheduleBreakdown(request.TeamSeasonId, request.IncludeDivision, cancellationToken);
    }
}