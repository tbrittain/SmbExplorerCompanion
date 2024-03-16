using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPitcherCareerRepository
{
    public Task<List<PlayerCareerPitchingDto>> GetPitchingCareers(
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        bool onlyHallOfFamers = false,
        int? pitcherRoleId = null,
        bool onlyActivePlayers = false,
        CancellationToken cancellationToken = default);
    
    public Task<List<PlayerCareerPitchingDto>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default);
    
    public Task<List<SimilarPlayerDto>> GetSimilarPitchingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}