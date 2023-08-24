using OneOf;
using SmbExplorerCompanion.Core.Entities.Summary;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISummaryRepository
{
    public Task<OneOf<FranchiseSummaryDto, Exception>> GetFranchiseSummaryAsync();
}