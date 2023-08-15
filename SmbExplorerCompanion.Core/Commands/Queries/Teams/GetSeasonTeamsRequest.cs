using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetSeasonTeamsRequest : IRequest<OneOf<IEnumerable<TeamDto>, Exception>>
{
    public GetSeasonTeamsRequest(int seasonId)
    {
        SeasonId = seasonId;
    }

    private int SeasonId { get; }

    // ReSharper disable once UnusedType.Global
    public class GetSeasonTeamsHandler : IRequestHandler<GetSeasonTeamsRequest, OneOf<IEnumerable<TeamDto>, Exception>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetSeasonTeamsHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<OneOf<IEnumerable<TeamDto>, Exception>> Handle(GetSeasonTeamsRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetSeasonTeams(request.SeasonId, cancellationToken);
    }
}