using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Exceptions;
using SmbExplorerCompanion.Core.ValueObjects.Progress;
using SmbExplorerCompanion.Csv.Services;

namespace SmbExplorerCompanion.Database.Services;

public class CsvImportRepository : ICsvImportRepository
{
    private readonly CsvMappingRepository _csvMappingRepository;
    private readonly CsvReaderService _csvReaderService;
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public CsvImportRepository(CsvMappingRepository csvMappingRepository,
        CsvReaderService csvReaderService,
        SmbExplorerCompanionDbContext dbContext)
    {
        _csvMappingRepository = csvMappingRepository;
        _csvReaderService = csvReaderService;
        _dbContext = dbContext;
    }

    public async Task ImportSeason(ImportSeasonFilePaths filePaths, ChannelWriter<ImportProgress> channel, CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths) ValidateFile(filePath);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await ImportTeams(filePaths.Teams, channel, cancellationToken);

            await ImportOverallPlayers(filePaths.OverallPlayers, channel, cancellationToken);

            await ImportSeasonStatsPitching(filePaths.SeasonStatsPitching, channel, cancellationToken);

            await ImportSeasonStatsBatting(filePaths.SeasonStatsBatting, channel, cancellationToken);

            await ImportSeasonSchedule(filePaths.SeasonSchedule, channel, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            channel.Complete();
        }
    }

    public async Task ImportPlayoffs(ImportPlayoffFilePaths filePaths, ChannelWriter<ImportProgress> channel, CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths) ValidateFile(filePath);

        var seasonId = await _csvReaderService.GetSeasonIdFromPlayoffPitching(filePaths.PlayoffStatsPitching);
        var season = await _dbContext.Seasons
            .SingleOrDefaultAsync(x => x.Id == seasonId, cancellationToken: cancellationToken);

        if (season is null)
            throw new Exception("Season not found. Please import the regular season data first.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await ImportPlayoffStatsPitching(filePaths.PlayoffStatsPitching, channel, cancellationToken);

            await ImportPlayoffStatsBatting(filePaths.PlayoffStatsBatting, channel, cancellationToken);

            await ImportPlayoffSchedule(filePaths.PlayoffSchedule, channel, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            channel.Complete();
        }
    }

    private async Task ImportTeams(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var teams = await _csvReaderService.ReadTeamsAsync(filePath);
        await _csvMappingRepository.AddTeamsAsync(teams, channelWriter, cancellationToken);
    }

    private async Task ImportOverallPlayers(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var players = await _csvReaderService.ReadOverallPlayersAsync(filePath);
        await _csvMappingRepository.AddOverallPlayersAsync(players, channelWriter, cancellationToken);
    }

    private async Task ImportSeasonStatsPitching(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await _csvMappingRepository.AddPlayerPitchingStatsAsync(stats, channelWriter, true, cancellationToken);
    }

    private async Task ImportSeasonStatsBatting(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await _csvMappingRepository.AddPlayerBattingStatsAsync(stats, channelWriter, true, cancellationToken);
    }

    private async Task ImportSeasonSchedule(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var schedule = await _csvReaderService.ReadSeasonScheduleAsync(filePath);
        await _csvMappingRepository.AddSeasonScheduleAsync(schedule, channelWriter, cancellationToken);
    }

    private async Task ImportPlayoffStatsPitching(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await _csvMappingRepository.AddPlayerPitchingStatsAsync(stats, channelWriter, false, cancellationToken);
    }

    private async Task ImportPlayoffStatsBatting(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await _csvMappingRepository.AddPlayerBattingStatsAsync(stats, channelWriter, false, cancellationToken);
    }

    private async Task ImportPlayoffSchedule(string filePath, ChannelWriter<ImportProgress> channelWriter, CancellationToken cancellationToken)
    {
        var schedule = await _csvReaderService.ReadPlayoffScheduleAsync(filePath);
        await _csvMappingRepository.AddPlayoffScheduleAsync(schedule, channelWriter, cancellationToken);
    }

    private static void ValidateFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var extension = Path.GetExtension(filePath);
        if (extension != ".csv")
            throw new FileFormatException($"File is not a csv: {filePath}");
    }
}