using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Exceptions;
using SmbExplorerCompanion.Core.ValueObjects.Progress;
using SmbExplorerCompanion.Csv.Services;
using SmbExplorerCompanion.Database.Entities;

namespace SmbExplorerCompanion.Database.Services.Imports;

public class CsvImportRepository(
    CsvMappingRepository csvMappingRepository,
    CsvReaderService csvReaderService,
    SmbExplorerCompanionDbContext dbContext,
    IApplicationContext applicationContext)
    : ICsvImportRepository
{
    public async Task<SeasonDto> ImportSeason(ImportSeasonFilePaths filePaths,
        ChannelWriter<ImportProgress> channel,
        SeasonDto selectedSeason,
        CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths) ValidateFile(filePath);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        Season season;
        try
        {
            season = await GetOrCreateSeason(selectedSeason, cancellationToken);
            
            await ImportTeams(filePaths.Teams, channel, season, cancellationToken);

            await ImportOverallPlayers(filePaths.OverallPlayers, channel, season, cancellationToken);

            await ImportSeasonStatsPitching(filePaths.SeasonStatsPitching, channel, season, cancellationToken);

            await ImportSeasonStatsBatting(filePaths.SeasonStatsBatting, channel, season, cancellationToken);

            await ImportSeasonSchedule(filePaths.SeasonSchedule, channel, season, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            applicationContext.HasFranchiseData = true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            throw;
        }
        finally
        {
            channel.Complete();
        }

        if (selectedSeason.Id == default)
        {
            selectedSeason.Id = season.Id;
        }
        
        return selectedSeason;
    }

    private async Task<Season> GetOrCreateSeason(SeasonDto selectedSeason, CancellationToken cancellationToken)
    {
        Season season;
        if (selectedSeason.Id == default)
        {
            var atLeastOneSeasonExists = await dbContext.Seasons
                .AnyAsync(cancellationToken: cancellationToken);

            var maxSeasonId = 0;
            if (atLeastOneSeasonExists)
            {
                maxSeasonId = await dbContext.Seasons
                    .MaxAsync(x => x.Id, cancellationToken: cancellationToken);
            }

            season = new Season
            {
                Id = maxSeasonId + 1,
                FranchiseId = applicationContext.SelectedFranchiseId!.Value,
                Number = selectedSeason.Number,
            };
            dbContext.Seasons.Add(season);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            season = await dbContext.Seasons
                         .SingleOrDefaultAsync(x => x.Id == selectedSeason.Id, cancellationToken: cancellationToken) ??
                     throw new Exception($"Season not found: {selectedSeason.Id}");
        }

        return season;
    }

    public async Task ImportPlayoffs(ImportPlayoffFilePaths filePaths,
        ChannelWriter<ImportProgress> channel,
        SeasonDto selectedSeason,
        CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths) ValidateFile(filePath);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var season = await GetOrCreateSeason(selectedSeason, cancellationToken);

            await ImportPlayoffStatsPitching(filePaths.PlayoffStatsPitching, channel, season, cancellationToken);

            await ImportPlayoffStatsBatting(filePaths.PlayoffStatsBatting, channel, season, cancellationToken);

            await ImportPlayoffSchedule(filePaths.PlayoffSchedule, channel, season, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            throw;
        }
        finally
        {
            channel.Complete();
        }
    }

    private async Task ImportTeams(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var teams = await csvReaderService.ReadTeamsAsync(filePath);
        await csvMappingRepository.AddTeamsAsync(teams, channelWriter, season, cancellationToken);
    }

    private async Task ImportOverallPlayers(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var players = await csvReaderService.ReadOverallPlayersAsync(filePath);
        await csvMappingRepository.AddOverallPlayersAsync(players, channelWriter, season, cancellationToken);
    }

    private async Task ImportSeasonStatsPitching(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await csvMappingRepository.AddPlayerPitchingStatsAsync(stats, channelWriter, season, true, cancellationToken);
    }

    private async Task ImportSeasonStatsBatting(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await csvMappingRepository.AddPlayerBattingStatsAsync(stats, channelWriter, season, true, cancellationToken);
    }

    private async Task ImportSeasonSchedule(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var schedule = await csvReaderService.ReadSeasonScheduleAsync(filePath);
        await csvMappingRepository.AddSeasonScheduleAsync(schedule, channelWriter, season, cancellationToken);
    }

    private async Task ImportPlayoffStatsPitching(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await csvMappingRepository.AddPlayerPitchingStatsAsync(stats, channelWriter, season, false, cancellationToken);
    }

    private async Task ImportPlayoffStatsBatting(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await csvMappingRepository.AddPlayerBattingStatsAsync(stats, channelWriter, season, false, cancellationToken);
    }

    private async Task ImportPlayoffSchedule(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var schedule = await csvReaderService.ReadPlayoffScheduleAsync(filePath);
        await csvMappingRepository.AddPlayoffScheduleAsync(schedule, channelWriter, season, cancellationToken);
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