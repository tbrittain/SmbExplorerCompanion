using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record Chemistry : LookupBase;

public static class ChemistryExtensions
{
    public static Chemistry FromCore(this ChemistryDto chemistryDto)
    {
        return new Chemistry
        {
            Id = chemistryDto.Id,
            Name = chemistryDto.Name
        };
    }
}