using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ICsvImportRepository
{
    public Task ImportSeason(ImportSeasonFilePaths filePaths, int franchiseId, CancellationToken cancellationToken);
    public Task ImportPlayoffs(ImportPlayoffFilePaths filePaths, CancellationToken cancellationToken);
}