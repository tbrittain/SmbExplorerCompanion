using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.WPF.Models;

namespace SmbExplorerCompanion.WPF.Mappings;

[Mapper]
public partial class FranchiseMapping
{
    public partial Franchise FromDto(FranchiseDto franchiseDto);
}