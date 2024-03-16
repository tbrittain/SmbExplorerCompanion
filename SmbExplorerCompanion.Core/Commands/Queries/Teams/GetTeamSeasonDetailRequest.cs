using MediatR;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetTeamSeasonDetailRequest : IRequest<TeamSeasonDetailDto>
{
    public GetTeamSeasonDetailRequest(int teamSeasonId)
    {
        TeamSeasonId = teamSeasonId;
    }

    private int TeamSeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTeamSeasonDetailHandler : IRequestHandler<GetTeamSeasonDetailRequest, TeamSeasonDetailDto>
    {
        private readonly ITeamRepository _teamRepository;

        public GetTeamSeasonDetailHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<TeamSeasonDetailDto> Handle(GetTeamSeasonDetailRequest request, CancellationToken cancellationToken)
        {
            return await _teamRepository.GetTeamSeasonDetail(request.TeamSeasonId, cancellationToken);
        }
    }
}