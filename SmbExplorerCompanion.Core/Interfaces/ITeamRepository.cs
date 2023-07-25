using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ITeamRepository
{
    public Task<OneOf<IEnumerable<HistoricalTeamDto>, Exception>> GetHistoricalTeams(CancellationToken cancellationToken);
}