using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetHistoricalTeamsRequest : IRequest<OneOf<IEnumerable<HistoricalTeamDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetHistoricalTeamsHandler : IRequestHandler<GetHistoricalTeamsRequest, OneOf<IEnumerable<HistoricalTeamDto>, Exception>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetHistoricalTeamsHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<OneOf<IEnumerable<HistoricalTeamDto>, Exception>> Handle(GetHistoricalTeamsRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetHistoricalTeams(cancellationToken);
    }
}