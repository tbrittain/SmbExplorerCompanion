using OneOf;
using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ITeamRepository
{
    public Task<OneOf<IEnumerable<TeamDto>, Exception>> GetSeasonTeams(int seasonId, CancellationToken cancellationToken);
    public Task<OneOf<IEnumerable<HistoricalTeamDto>, Exception>> GetHistoricalTeams(int? seasonId, CancellationToken cancellationToken);
    public Task<OneOf<TeamOverviewDto, Exception>> GetTeamOverview(int teamId, CancellationToken cancellationToken);
    public Task<OneOf<TeamSeasonDetailDto, Exception>> GetTeamSeasonDetail(int teamSeasonId, CancellationToken cancellationToken);
}