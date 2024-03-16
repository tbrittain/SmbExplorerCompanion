using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ITeamRepository
{
    public Task<IEnumerable<TeamDto>> GetSeasonTeams(int seasonId, CancellationToken cancellationToken);
    public Task<IEnumerable<HistoricalTeamDto>> GetHistoricalTeams(int? seasonId, CancellationToken cancellationToken);
    public Task<TeamOverviewDto> GetTeamOverview(int teamId, CancellationToken cancellationToken);
    public Task<TeamSeasonDetailDto> GetTeamSeasonDetail(int teamSeasonId, CancellationToken cancellationToken);

    public Task<DivisionScheduleBreakdownDto> GetTeamScheduleBreakdown(int teamSeasonId,
        bool includeDivision,
        CancellationToken cancellationToken);
}