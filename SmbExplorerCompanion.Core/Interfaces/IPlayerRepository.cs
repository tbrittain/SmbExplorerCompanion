using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPlayerRepository
{
    public Task<OneOf<PlayerOverviewDto, Exception>> GetHistoricalPlayer(int playerId,
        CancellationToken cancellationToken = default);
    public Task<OneOf<List<PlayerCareerDto>, Exception>> GetPositionPlayerCareers(int pageNumber,
        string? orderBy, bool ascending = true, CancellationToken cancellationToken = default);
}