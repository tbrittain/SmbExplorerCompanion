using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Database.Services;

public class AwardRepository : IAwardRepository
{
    public async Task<OneOf<Success, Exception>> AddPlayerAwards(List<PlayerAwardRequestDto> playerAwardRequestDtos, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}