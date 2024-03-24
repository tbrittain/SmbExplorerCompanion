using MediatR;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetHistoricalTeamsRequest : IRequest<IEnumerable<HistoricalTeamDto>>
{
    public GetHistoricalTeamsRequest(SeasonRange seasonRange)
    {
        SeasonRange = seasonRange;
    }

    private SeasonRange SeasonRange { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetHistoricalTeamsHandler : IRequestHandler<GetHistoricalTeamsRequest, IEnumerable<HistoricalTeamDto>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetHistoricalTeamsHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<IEnumerable<HistoricalTeamDto>> Handle(GetHistoricalTeamsRequest request,
            CancellationToken cancellationToken)
        {
            return await _teamRepository.GetHistoricalTeams(request.SeasonRange, cancellationToken);
        }
    }
}