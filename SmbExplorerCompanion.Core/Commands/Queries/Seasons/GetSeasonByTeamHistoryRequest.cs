using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Seasons;

public class GetSeasonByTeamHistoryRequest : IRequest<OneOf<SeasonDto, None, Exception>>
{
    public GetSeasonByTeamHistoryRequest(int teamSeasonId)
    {
        TeamSeasonId = teamSeasonId;
    }

    private int TeamSeasonId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetSeasonByTeamHistoryHandler : IRequestHandler<GetSeasonByTeamHistoryRequest, OneOf<SeasonDto, None, Exception>>
    {
        private readonly ISeasonSearchService _seasonSearchService;

        public GetSeasonByTeamHistoryHandler(ISeasonSearchService seasonSearchService)
        {
            _seasonSearchService = seasonSearchService;
        }

        public async Task<OneOf<SeasonDto, None, Exception>> Handle(GetSeasonByTeamHistoryRequest request, CancellationToken cancellationToken) =>
            await _seasonSearchService.GetByTeamSeasonIdAsync(request.TeamSeasonId, cancellationToken);
    }
}