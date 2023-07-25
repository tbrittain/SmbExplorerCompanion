using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetHistoricalTeamsRequest : IRequest<OneOf<IEnumerable<HistoricalTeam>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    public class GetHistoricalTeamsHandler : IRequestHandler<GetHistoricalTeamsRequest, OneOf<IEnumerable<HistoricalTeam>, Exception>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetHistoricalTeamsHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<OneOf<IEnumerable<HistoricalTeam>, Exception>> Handle(GetHistoricalTeamsRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetHistoricalTeams(cancellationToken);
    }
}