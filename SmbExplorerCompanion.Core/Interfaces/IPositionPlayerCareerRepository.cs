using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.ValueObjects.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public record GetBattingCareersFilters : PlayerFilters
{
    public bool OnlyHallOfFamers { get; init; } = false;
    public int? PrimaryPositionId { get; init; }
    public bool OnlyActivePlayers { get; init; } = false;
}

public interface IPositionPlayerCareerRepository
{
    public Task<List<PlayerCareerBattingDto>> GetBattingCareers(
        GetBattingCareersFilters filters,
        CancellationToken cancellationToken = default);

    public Task<List<PlayerCareerBattingDto>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default);

    public Task<List<SimilarPlayerDto>> GetSimilarBattingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}