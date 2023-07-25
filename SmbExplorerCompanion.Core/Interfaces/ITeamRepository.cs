using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ITeamRepository
{
    public Task<IEnumerable<HistoricalTeam>> GetHistoricalTeams();
}