namespace SmbExplorerCompanion.Core.Interfaces;

public interface ICsvImportRepository
{
    public Task ImportTeams(string filePath, CancellationToken cancellationToken);
    public Task ImportOverallPlayers(string filePath, CancellationToken cancellationToken);
    public Task ImportSeasonStatsPitching(string filePath, CancellationToken cancellationToken);
    public Task ImportSeasonStatsBatting(string filePath, CancellationToken cancellationToken);
    public Task ImportSeasonSchedule(string filePath, CancellationToken cancellationToken);
    public Task ImportPlayoffStatsPitching(string filePath, CancellationToken cancellationToken);
    public Task ImportPlayoffStatsBatting(string filePath, CancellationToken cancellationToken);
    public Task ImportPlayoffSchedule(string filePath, CancellationToken cancellationToken);
}