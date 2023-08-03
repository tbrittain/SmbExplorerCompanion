using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ITeamRepository
{
    public Task<OneOf<IEnumerable<HistoricalTeamDto>, Exception>> GetHistoricalTeams(CancellationToken cancellationToken);
    public Task<OneOf<TeamOverviewDto, Exception>> GetTeamOverview(int teamId, CancellationToken cancellationToken);
    public Task<OneOf<TeamSeasonDetailDto, Exception>> GetTeamSeasonDetail(int teamSeasonId, CancellationToken cancellationToken);
}