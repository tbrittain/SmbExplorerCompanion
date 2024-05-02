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

public class CsvImportRepository : ICsvImportRepository
{
    private readonly CsvMappingRepository _csvMappingRepository;
    private readonly CsvReaderService _csvReaderService;
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IApplicationContext _applicationContext;

    public CsvImportRepository(CsvMappingRepository csvMappingRepository,
        CsvReaderService csvReaderService,
        SmbExplorerCompanionDbContext dbContext,
        IApplicationContext applicationContext)
    {
        _csvMappingRepository = csvMappingRepository;
        _csvReaderService = csvReaderService;
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    public async Task<SeasonDto> ImportSeason(ImportSeasonFilePaths filePaths,
        ChannelWriter<ImportProgress> channel,
        SeasonDto selectedSeason,
        CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths) ValidateFile(filePath);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

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

            _applicationContext.HasFranchiseData = true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
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
            var atLeastOneSeasonExists = await _dbContext.Seasons
                .AnyAsync(cancellationToken: cancellationToken);

            var maxSeasonId = 0;
            if (atLeastOneSeasonExists)
            {
                maxSeasonId = await _dbContext.Seasons
                    .MaxAsync(x => x.Id, cancellationToken: cancellationToken);
            }

            season = new Season
            {
                Id = maxSeasonId + 1,
                FranchiseId = _applicationContext.SelectedFranchiseId!.Value,
                Number = selectedSeason.Number,
            };
            _dbContext.Seasons.Add(season);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            season = await _dbContext.Seasons
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

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

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
            _dbContext.ChangeTracker.Clear();
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
        var teams = await _csvReaderService.ReadTeamsAsync(filePath);
        await _csvMappingRepository.AddTeamsAsync(teams, channelWriter, season, cancellationToken);
    }

    private async Task ImportOverallPlayers(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var players = await _csvReaderService.ReadOverallPlayersAsync(filePath);
        await _csvMappingRepository.AddOverallPlayersAsync(players, channelWriter, season, cancellationToken);
    }

    private async Task ImportSeasonStatsPitching(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await _csvMappingRepository.AddPlayerPitchingStatsAsync(stats, channelWriter, season, true, cancellationToken);
    }

    private async Task ImportSeasonStatsBatting(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await _csvMappingRepository.AddPlayerBattingStatsAsync(stats, channelWriter, season, true, cancellationToken);
    }

    private async Task ImportSeasonSchedule(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var schedule = await _csvReaderService.ReadSeasonScheduleAsync(filePath);
        await _csvMappingRepository.AddSeasonScheduleAsync(schedule, channelWriter, season, cancellationToken);
    }

    private async Task ImportPlayoffStatsPitching(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await _csvMappingRepository.AddPlayerPitchingStatsAsync(stats, channelWriter, season, false, cancellationToken);
    }

    private async Task ImportPlayoffStatsBatting(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var stats = await _csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await _csvMappingRepository.AddPlayerBattingStatsAsync(stats, channelWriter, season, false, cancellationToken);
    }

    private async Task ImportPlayoffSchedule(string filePath,
        ChannelWriter<ImportProgress> channelWriter,
        Season season,
        CancellationToken cancellationToken)
    {
        var schedule = await _csvReaderService.ReadPlayoffScheduleAsync(filePath);
        await _csvMappingRepository.AddPlayoffScheduleAsync(schedule, channelWriter, season, cancellationToken);
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