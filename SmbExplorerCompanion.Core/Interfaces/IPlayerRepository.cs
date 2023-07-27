using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPlayerRepository
{
    public Task<OneOf<PlayerOverview, Exception>> GetHistoricalPlayer(int playerId, CancellationToken cancellationToken);
}