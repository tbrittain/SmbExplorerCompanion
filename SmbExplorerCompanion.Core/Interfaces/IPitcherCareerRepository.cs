using SmbExplorerCompanion.Core.Entities.Players;
using OneOf;
using OneOf.Types;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPitcherCareerRepository
{
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
    
    public Task<OneOf<List<PlayerCareerPitchingDto>, None, Exception>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default);
    
    public Task<OneOf<List<SimilarPlayerDto>, Exception>> GetSimilarPitchingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}