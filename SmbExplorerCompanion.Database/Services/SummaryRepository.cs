using OneOf;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class SummaryRepository : ISummaryRepository
{
    public Task<OneOf<FranchiseSummaryDto, Exception>> GetFranchiseSummaryAsync()
    {
        throw new NotImplementedException();
    }
}