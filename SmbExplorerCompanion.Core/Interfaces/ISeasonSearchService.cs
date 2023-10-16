using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Seasons;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISeasonSearchService
{
    public Task<OneOf<SeasonDto, None, Exception>> GetByTeamSeasonIdAsync(int teamSeasonId, CancellationToken cancellationToken = default);
}