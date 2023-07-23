using MediatR;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Commands.Actions.Csv;

public class ImportSeasonDataRequest : IRequest<OneOf<Success, Exception>>
{
    public ImportSeasonDataRequest(
        int franchiseId,
        string teamsCsvFilePath,
        string overallPlayersCsvFilePath,
        string seasonStatsPitchingCsvFilePath,
        string seasonStatsBattingCsvFilePath,
        string seasonScheduleCsvFilePath)
    {
        FranchiseId = franchiseId;
        TeamsCsvFilePath = teamsCsvFilePath;
        OverallPlayersCsvFilePath = overallPlayersCsvFilePath;
        SeasonStatsPitchingCsvFilePath = seasonStatsPitchingCsvFilePath;
        SeasonStatsBattingCsvFilePath = seasonStatsBattingCsvFilePath;
        SeasonScheduleCsvFilePath = seasonScheduleCsvFilePath;
    }

    private int FranchiseId { get; }
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
            var filePaths = new ImportSeasonFilePaths(
                request.TeamsCsvFilePath,
                request.OverallPlayersCsvFilePath,
                request.SeasonStatsPitchingCsvFilePath,
                request.SeasonStatsBattingCsvFilePath,
                request.SeasonScheduleCsvFilePath);

            try
            {
                await _csvImportRepository.ImportSeason(filePaths, request.FranchiseId, cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }

            return new Success();
        }
    }
}