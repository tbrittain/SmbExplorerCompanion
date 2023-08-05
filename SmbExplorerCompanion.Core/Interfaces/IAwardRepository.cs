using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IAwardRepository
{
    public Task<OneOf<Success, Exception>> AddPlayerAwards(List<PlayerAwardRequestDto> playerAwardRequestDtos,
        CancellationToken cancellationToken = default);
}