using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Database.Entities;

namespace SmbExplorerCompanion.Database.Mappings;

[Mapper]
public partial class FranchiseMapping
{
    public partial FranchiseDto FranchiseToFranchiseDto(Franchise franchise);
    public partial Franchise FranchiseDtoToFranchise(FranchiseDto franchiseDto);
}