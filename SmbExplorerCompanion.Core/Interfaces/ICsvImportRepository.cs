using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ICsvImportRepository
{
    public Task ImportSeason(ImportSeasonFilePaths filePaths, CancellationToken cancellationToken);
    public Task ImportPlayoffs(ImportPlayoffFilePaths filePaths, CancellationToken cancellationToken);
}