using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Summary;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISummaryRepository
{
    public Task<OneOf<FranchiseSummaryDto, None, Exception>> GetFranchiseSummaryAsync(CancellationToken cancellationToken = default);
    public Task<OneOf<List<ConferenceSummaryDto>, None, Exception>> GetLeagueSummaryAsync(CancellationToken cancellationToken = default);
}