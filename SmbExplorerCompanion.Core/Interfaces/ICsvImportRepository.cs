using System.Threading.Channels;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Progress;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ICsvImportRepository
{
    public Task ImportSeason(ImportSeasonFilePaths filePaths, ChannelWriter<ImportProgress> channel, CancellationToken cancellationToken);
    public Task ImportPlayoffs(ImportPlayoffFilePaths filePaths, ChannelWriter<ImportProgress> channel, CancellationToken cancellationToken);
}