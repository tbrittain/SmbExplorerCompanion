using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetLeagueAverageGameStatsRequest : IRequest<GameStatDto>
{
    public GetLeagueAverageGameStatsRequest(int seasonId, bool isPitcher, int? pitcherRoleId)
    {
        SeasonId = seasonId;
        IsPitcher = isPitcher;
        PitcherRoleId = pitcherRoleId;
    }

    private int SeasonId { get; }
    private bool IsPitcher { get; }
    private int? PitcherRoleId { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetLeagueAverageGameStatsHandler : IRequestHandler<GetLeagueAverageGameStatsRequest, GameStatDto>
    {
        private readonly IGeneralPlayerRepository _generalPlayerRepository;

        public GetLeagueAverageGameStatsHandler(IGeneralPlayerRepository generalPlayerRepository)
        {
            _generalPlayerRepository = generalPlayerRepository;
        }

        public async Task<GameStatDto> Handle(GetLeagueAverageGameStatsRequest request, CancellationToken cancellationToken) =>
            await _generalPlayerRepository.GetLeagueAverageGameStats(request.SeasonId,
                request.IsPitcher,
                request.PitcherRoleId,
                cancellationToken);
    }
}