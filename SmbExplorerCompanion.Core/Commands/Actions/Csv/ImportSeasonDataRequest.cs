using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Actions.Csv;

public class ImportSeasonDataRequest : IRequest<OneOf<Success, Exception>>
{
    public ImportSeasonDataRequest(
        string teamsCsvFilePath,
        string overallPlayersCsvFilePath,
        string seasonStatsPitchingCsvFilePath,
        string seasonStatsBattingCsvFilePath,
        string seasonScheduleCsvFilePath)
    {
        TeamsCsvFilePath = teamsCsvFilePath;
        OverallPlayersCsvFilePath = overallPlayersCsvFilePath;
        SeasonStatsPitchingCsvFilePath = seasonStatsPitchingCsvFilePath;
        SeasonStatsBattingCsvFilePath = seasonStatsBattingCsvFilePath;
        SeasonScheduleCsvFilePath = seasonScheduleCsvFilePath;
    }

    private string TeamsCsvFilePath { get; }
    private string OverallPlayersCsvFilePath { get; }
    private string SeasonStatsPitchingCsvFilePath { get; }
    private string SeasonStatsBattingCsvFilePath { get; }
    private string SeasonScheduleCsvFilePath { get; }

    // ReSharper disable once UnusedType.Global
    public class ImportSeasonDataHandler : IRequestHandler<ImportSeasonDataRequest, OneOf<Success, Exception>>
    {
        private readonly ICsvImportRepository _csvImportRepository;

        public ImportSeasonDataHandler(ICsvImportRepository csvImportRepository)
        {
            _csvImportRepository = csvImportRepository;
        }

        public async Task<OneOf<Success, Exception>> Handle(ImportSeasonDataRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _csvImportRepository.ImportTeams(request.TeamsCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }

            try
            {
                await _csvImportRepository.ImportOverallPlayers(request.OverallPlayersCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
            
            try
            {
                await _csvImportRepository.ImportSeasonStatsPitching(request.SeasonStatsPitchingCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
            
            try
            {
                await _csvImportRepository.ImportSeasonStatsBatting(request.SeasonStatsBattingCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
            
            try
            {
                await _csvImportRepository.ImportSeasonSchedule(request.SeasonScheduleCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
            
            return new Success();
        }
    }
}