using MediatR;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Teams;

public class GetHistoricalTeamsRequest : IRequest<IEnumerable<HistoricalTeamDto>>
{
    public GetHistoricalTeamsRequest(int? seasonId)
    {
        SeasonId = seasonId;
    }

    private int? SeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetHistoricalTeamsHandler : IRequestHandler<GetHistoricalTeamsRequest, IEnumerable<HistoricalTeamDto>>
    {
        private readonly ITeamRepository _teamRepository;

        public GetHistoricalTeamsHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<IEnumerable<HistoricalTeamDto>> Handle(GetHistoricalTeamsRequest request,
            CancellationToken cancellationToken) =>
            await _teamRepository.GetHistoricalTeams(request.SeasonId, cancellationToken);
    }
}