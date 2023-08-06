using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPlayerRepository
{
    public Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerCareerDto>, Exception>> GetTopBattingCareers(int? pageNumber,
        string? orderBy,
        bool descending = true,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerCareerDto>, Exception>> GetTopPitchingCareers(int? pageNumber,
        string? orderBy,
        bool descending = true,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> GetTopBattingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        int? primaryPositionId = null,
        bool onlyRookies = false,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> GetTopPitchingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        bool onlyRookies = false,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerFieldingRankingDto>, Exception>> GetPlayerFieldingRankings(int seasonId,
        int? primaryPositionId, int? pageNumber, int? limit, CancellationToken cancellationToken = default);
}