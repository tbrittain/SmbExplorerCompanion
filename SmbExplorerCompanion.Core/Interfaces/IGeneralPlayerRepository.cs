using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IGeneralPlayerRepository
{
    public Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerFieldingRankingDto>, Exception>> GetPlayerFieldingRankings(int seasonId,
        int? primaryPositionId,
        int? pageNumber,
        int? limit,
        CancellationToken cancellationToken = default);

    public Task<OneOf<GameStatDto, Exception>> GetLeagueAverageGameStats(int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);

    public Task<OneOf<PlayerGameStatPercentileDto, Exception>> GetPlayerGameStatPercentiles(int playerId,
        int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);
    
    public Task<OneOf<PlayerKpiPercentileDto, Exception>> GetPlayerKpiPercentiles(
        int playerId,
        int seasonId,
        bool isPitcher,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);

    public Task<OneOf<PlayerBaseDto, Exception>> GetRandomPlayer();
}