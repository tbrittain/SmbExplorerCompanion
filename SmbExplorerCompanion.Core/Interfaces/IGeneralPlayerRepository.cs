using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IGeneralPlayerRepository
{
    public Task<PlayerOverviewDto> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default);

    public Task<List<PlayerFieldingRankingDto>> GetPlayerFieldingRankings(int seasonId,
        int? primaryPositionId,
        int? pageNumber,
        int? limit,
        CancellationToken cancellationToken = default);

    public Task<GameStatDto> GetLeagueAverageGameStats(int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);

    public Task<PlayerGameStatPercentileDto> GetPlayerGameStatPercentiles(int playerId,
        int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);
    
    public Task<PlayerKpiPercentileDto> GetPlayerKpiPercentiles(
        int playerId,
        int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);

    public Task<PlayerBaseDto> GetRandomPlayer();
}