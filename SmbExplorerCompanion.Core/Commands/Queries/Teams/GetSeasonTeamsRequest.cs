using MediatR;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetSeasonTeamsRequest : IRequest<IEnumerable<TeamDto>>
{
    public GetSeasonTeamsRequest(int seasonId)
    {
        SeasonId = seasonId;
    }

    private int SeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetSeasonTeamsHandler : IRequestHandler<GetSeasonTeamsRequest, IEnumerable<TeamDto>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetSeasonTeamsHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<IEnumerable<TeamDto>> Handle(GetSeasonTeamsRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetSeasonTeams(request.SeasonId, cancellationToken);
    }
}