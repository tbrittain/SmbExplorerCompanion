using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IAwardDelegationRepository
{
    public Task<OneOf<Success, Exception>> AddRegularSeasonPlayerAwards(int seasonId,
        List<PlayerAwardRequestDto> playerAwardRequestDtos,
        CancellationToken cancellationToken = default);
}