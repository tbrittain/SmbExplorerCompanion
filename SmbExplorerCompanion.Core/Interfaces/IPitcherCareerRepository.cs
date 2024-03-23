using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.ValueObjects.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public record GetPitchingCareersFilters : SeasonPlayerFilters
{
    public bool OnlyHallOfFamers { get; init; } = false;
    public bool OnlyActivePlayers { get; init; } = false;
    public int? PitcherRoleId { get; init; }
}

public interface IPitcherCareerRepository
{
    public Task<List<PlayerCareerPitchingDto>> GetPitchingCareers(
        GetPitchingCareersFilters filters,
        CancellationToken cancellationToken = default);
    
    public Task<List<PlayerCareerPitchingDto>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default);
    
    public Task<List<SimilarPlayerDto>> GetSimilarPitchingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}