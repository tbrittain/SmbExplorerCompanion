using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Exceptions;
using SmbExplorerCompanion.Csv.Services;

namespace SmbExplorerCompanion.Database.Services;

public class CsvImportRepository : ICsvImportRepository
{
    private readonly CsvMappingRepository _csvMappingRepository;
    private readonly CsvReaderService _csvReaderService;

    public CsvImportRepository(CsvMappingRepository csvMappingRepository, CsvReaderService csvReaderService)
    {
        _csvMappingRepository = csvMappingRepository;
        _csvReaderService = csvReaderService;
    }

    public async Task ImportTeams(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportOverallPlayers(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportSeasonStatsPitching(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportSeasonStatsBatting(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportSeasonSchedule(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportPlayoffStatsPitching(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportPlayoffStatsBatting(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
    }

    public async Task ImportPlayoffSchedule(string filePath, CancellationToken cancellationToken)
    {
        ValidateFile(filePath);
        throw new NotImplementedException();
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