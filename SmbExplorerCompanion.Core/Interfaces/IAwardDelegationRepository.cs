using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IAwardDelegationRepository
{
    public Task AddRegularSeasonPlayerAwards(int seasonId,
        List<PlayerAwardRequestDto> playerAwardRequestDtos,
        CancellationToken cancellationToken = default);

    public Task AddHallOfFameAwards(List<PlayerHallOfFameRequestDto> players,
        CancellationToken cancellationToken = default);
}