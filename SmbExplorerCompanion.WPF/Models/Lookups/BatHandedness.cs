using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record BatHandedness : LookupBase;

public static class BatHandednessExtensions
{
    public static BatHandedness FromCore(this BatHandednessDto batHandednessDto)
    {
        return new BatHandedness
        {
            Id = batHandednessDto.Id,
            Name = batHandednessDto.Name
        };
    }
}