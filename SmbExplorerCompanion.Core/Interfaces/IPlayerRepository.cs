using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPlayerRepository
{
    public Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerCareerBattingDto>, Exception>> GetBattingCareers(
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        bool onlyHallOfFamers = false,
        int? primaryPositionId = null,
        bool onlyActivePlayers = false,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerCareerPitchingDto>, Exception>> GetPitchingCareers(
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        bool onlyHallOfFamers = false,
        int? pitcherRoleId = null,
        bool onlyActivePlayers = false,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerBattingSeasonDto>, Exception>> GetBattingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        int? primaryPositionId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false,
        int? playerId = null,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerPitchingSeasonDto>, Exception>> GetPitchingSeasons(
        int? seasonId = null,
        bool isPlayoffs = false,
        int? pageNumber = null,
        string? orderBy = null,
        int? limit = null,
        bool descending = true,
        int? teamId = null,
        bool onlyRookies = false,
        bool includeChampionAwards = true,
        bool onlyUserAssignableAwards = false,
        int? playerId = null,
        int? pitcherRoleId = null,
        CancellationToken cancellationToken = default);

    public Task<OneOf<List<PlayerFieldingRankingDto>, Exception>> GetPlayerFieldingRankings(int seasonId,
        int? primaryPositionId, int? pageNumber, int? limit, CancellationToken cancellationToken = default);

    public Task<OneOf<RetiredPlayerCareerStatsDto, None, Exception>> GetHallOfFameCandidates(int seasonId, CancellationToken cancellationToken = default);
    
    public Task<OneOf<List<SimilarPlayerDto>, Exception>> GetSimilarBattingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}