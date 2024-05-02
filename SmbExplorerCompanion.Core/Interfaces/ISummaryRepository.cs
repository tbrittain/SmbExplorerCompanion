using SmbExplorerCompanion.Core.Entities.Summary;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ISummaryRepository
{
    public Task<bool> HasFranchiseDataAsync(CancellationToken cancellationToken = default);
    public Task<FranchiseSummaryDto?> GetFranchiseSummaryAsync(CancellationToken cancellationToken = default);
    public Task<List<ConferenceSummaryDto>> GetLeagueSummaryAsync(CancellationToken cancellationToken = default);
}