using SmbExplorerCompanion.Core.Entities.Seasons;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISeasonSearchService
{
    public Task<SeasonDto?> GetByTeamSeasonIdAsync(int teamSeasonId, CancellationToken cancellationToken = default);
}