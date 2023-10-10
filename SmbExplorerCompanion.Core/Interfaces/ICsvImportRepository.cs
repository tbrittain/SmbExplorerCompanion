using System.Threading.Channels;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Progress;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ICsvImportRepository
{
    public Task<SeasonDto> ImportSeason(ImportSeasonFilePaths filePaths,
        ChannelWriter<ImportProgress> channel,
        SeasonDto selectedSeason,
        CancellationToken cancellationToken);

    public Task ImportPlayoffs(ImportPlayoffFilePaths filePaths,
        ChannelWriter<ImportProgress> channel,
        SeasonDto selectedSeason,
        CancellationToken cancellationToken);
}