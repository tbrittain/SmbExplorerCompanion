﻿using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Exceptions;
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

    public async Task ImportSeason(ImportSeasonFilePaths filePaths, int franchiseId, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await ImportTeams(filePaths.Teams, franchiseId, cancellationToken);

            await ImportOverallPlayers(filePaths.OverallPlayers, cancellationToken);

            await ImportSeasonStatsPitching(filePaths.SeasonStatsPitching, cancellationToken);

            await ImportSeasonStatsBatting(filePaths.SeasonStatsBatting, cancellationToken);

            await ImportSeasonSchedule(filePaths.SeasonSchedule, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ImportPlayoffs(ImportPlayoffFilePaths filePaths, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await ImportPlayoffStatsPitching(filePaths.PlayoffStatsPitching, cancellationToken);

            await ImportPlayoffStatsBatting(filePaths.PlayoffStatsBatting, cancellationToken);

            await ImportPlayoffSchedule(filePaths.PlayoffSchedule, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ImportTeams(string filePath, int franchiseId, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var teams = await _csvReaderService.ReadTeamsAsync(filePath);
        await _csvMappingRepository.AddTeamsAsync(teams, franchiseId, cancellationToken);
    }

    private async Task ImportOverallPlayers(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var players = await _csvReaderService.ReadOverallPlayersAsync(filePath);
        await _csvMappingRepository.AddOverallPlayersAsync(players, cancellationToken);
    }

    private async Task ImportSeasonStatsPitching(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var stats = await _csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await _csvMappingRepository.AddPlayerPitchingStatsAsync(stats, true, cancellationToken);
    }

    private async Task ImportSeasonStatsBatting(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var stats = await _csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await _csvMappingRepository.AddPlayerBattingStatsAsync(stats, true, cancellationToken);
    }

    private async Task ImportSeasonSchedule(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var schedule = await _csvReaderService.ReadSeasonScheduleAsync(filePath);
        await _csvMappingRepository.AddSeasonScheduleAsync(schedule, cancellationToken);
    }

    private async Task ImportPlayoffStatsPitching(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var stats = await _csvReaderService.ReadPlayerStatPitchingAsync(filePath);
        await _csvMappingRepository.AddPlayerPitchingStatsAsync(stats, false, cancellationToken);
    }

    private async Task ImportPlayoffStatsBatting(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var stats = await _csvReaderService.ReadPlayerStatBattingAsync(filePath);
        await _csvMappingRepository.AddPlayerBattingStatsAsync(stats, false, cancellationToken);
    }

    private async Task ImportPlayoffSchedule(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);

        var schedule = await _csvReaderService.ReadPlayoffScheduleAsync(filePath);
        await _csvMappingRepository.AddPlayoffScheduleAsync(schedule, cancellationToken);
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