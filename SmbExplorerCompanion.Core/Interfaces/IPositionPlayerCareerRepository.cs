using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;
using OneOf.Types;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPositionPlayerCareerRepository
{
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
    
    public Task<OneOf<List<PlayerCareerBattingDto>, None, Exception>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default);
    
    public Task<OneOf<List<SimilarPlayerDto>, Exception>> GetSimilarBattingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}