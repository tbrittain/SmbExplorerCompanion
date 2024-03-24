using MediatR;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Seasons;

public class GetSeasonByTeamHistoryRequest : IRequest<SeasonDto?>
{
    public GetSeasonByTeamHistoryRequest(int teamSeasonId)
    {
        TeamSeasonId = teamSeasonId;
    }

    private int TeamSeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetSeasonByTeamHistoryHandler : IRequestHandler<GetSeasonByTeamHistoryRequest, SeasonDto?>
    {
        private readonly ISeasonSearchService _seasonSearchService;

        public GetSeasonByTeamHistoryHandler(ISeasonSearchService seasonSearchService)
        {
            _seasonSearchService = seasonSearchService;
        }

        public async Task<SeasonDto?> Handle(GetSeasonByTeamHistoryRequest request, CancellationToken cancellationToken)
        {
            return await _seasonSearchService.GetByTeamSeasonIdAsync(request.TeamSeasonId, cancellationToken);
        }
    }
}