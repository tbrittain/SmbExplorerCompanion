using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetTeamScheduleBreakdownRequest : IRequest<OneOf<HashSet<TeamScheduleBreakdownDto>, Exception>>
{
    public GetTeamScheduleBreakdownRequest(int teamSeasonId)
    {
        TeamSeasonId = teamSeasonId;
    }

    private int TeamSeasonId { get; }
    
    // ReSharper disable once UnusedType.Global
    internal class GetTeamScheduleBreakdownHandler : IRequestHandler<GetTeamScheduleBreakdownRequest, OneOf<HashSet<TeamScheduleBreakdownDto>, Exception>>
    {
        private readonly ITeamRepository _teamRepository;
        
        public GetTeamScheduleBreakdownHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<OneOf<HashSet<TeamScheduleBreakdownDto>, Exception>> Handle(GetTeamScheduleBreakdownRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetTeamScheduleBreakdown(request.TeamSeasonId, cancellationToken);
    }
}