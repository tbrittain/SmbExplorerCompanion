using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models;

public record Franchise : LookupBase;

public static class FranchiseExtensions
{
    public static Franchise FromCore(this FranchiseDto franchiseDto)
    {
        return new Franchise
        {
            Id = franchiseDto.Id,
            Name = franchiseDto.Name
        };
    }
}