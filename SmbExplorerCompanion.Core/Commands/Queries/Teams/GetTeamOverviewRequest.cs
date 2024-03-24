using MediatR;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetTeamOverviewRequest : IRequest<TeamOverviewDto>
{
    public GetTeamOverviewRequest(int teamId)
    {
        TeamId = teamId;
    }

    private int TeamId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTeamOverviewHandler : IRequestHandler<GetTeamOverviewRequest, TeamOverviewDto>
    {
        private readonly ITeamRepository _teamRepository;

        public GetTeamOverviewHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<TeamOverviewDto> Handle(GetTeamOverviewRequest request,
            CancellationToken cancellationToken)
        {
            return await _teamRepository.GetTeamOverview(request.TeamId, cancellationToken);
        }
    }
}