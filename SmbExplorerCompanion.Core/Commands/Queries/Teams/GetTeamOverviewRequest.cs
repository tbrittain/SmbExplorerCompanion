using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetTeamOverviewRequest : IRequest<OneOf<TeamOverviewDto, Exception>>
{
    public GetTeamOverviewRequest(int teamId)
    {
        TeamId = teamId;
    }

    private int TeamId { get; }
    
    // ReSharper disable once UnusedType.Global
    public class GetTeamOverviewHandler : IRequestHandler<GetTeamOverviewRequest, OneOf<TeamOverviewDto, Exception>>
    {
        private readonly ITeamRepository _teamRepository;
        
        public GetTeamOverviewHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<OneOf<TeamOverviewDto, Exception>> Handle(GetTeamOverviewRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetTeamOverview(request.TeamId, cancellationToken);
    }
}