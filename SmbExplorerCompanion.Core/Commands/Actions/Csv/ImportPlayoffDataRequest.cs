using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Actions.Csv;

public class ImportPlayoffDataRequest : IRequest<OneOf<Success, Exception>>
{
    public ImportPlayoffDataRequest(string playoffStatsPitchingCsvFilePath, string playoffStatsBattingCsvFilePath, string playoffScheduleCsvFilePath)
    {
        PlayoffStatsPitchingCsvFilePath = playoffStatsPitchingCsvFilePath;
        PlayoffStatsBattingCsvFilePath = playoffStatsBattingCsvFilePath;
        PlayoffScheduleCsvFilePath = playoffScheduleCsvFilePath;
    }

    private string PlayoffStatsPitchingCsvFilePath { get; }
    private string PlayoffStatsBattingCsvFilePath { get; }
    private string PlayoffScheduleCsvFilePath { get; }
    
    // ReSharper disable once UnusedType.Global
    public class ImportPlayoffDataHandler : IRequestHandler<ImportPlayoffDataRequest, OneOf<Success, Exception>>
    {
        private readonly ICsvImportRepository _csvImportRepository;

        public ImportPlayoffDataHandler(ICsvImportRepository csvImportRepository)
        {
            _csvImportRepository = csvImportRepository;
        }

        public async Task<OneOf<Success, Exception>> Handle(ImportPlayoffDataRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _csvImportRepository.ImportPlayoffStatsPitching(request.PlayoffStatsPitchingCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }

            try
            {
                await _csvImportRepository.ImportPlayoffStatsBatting(request.PlayoffStatsBattingCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }

            try
            {
                await _csvImportRepository.ImportPlayoffSchedule(request.PlayoffScheduleCsvFilePath, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }

            return new Success();
        }
    }
}