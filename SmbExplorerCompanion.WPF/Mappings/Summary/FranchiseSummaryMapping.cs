using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.WPF.Models.Summary;

namespace SmbExplorerCompanion.WPF.Mappings.Summary;

[Mapper]
public partial class FranchiseSummaryMapping
{
    public partial FranchiseSummary FromFranchiseSummaryDto(FranchiseSummaryDto franchiseSummaryDto);
}