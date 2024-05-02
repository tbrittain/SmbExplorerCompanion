using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record ThrowHandedness : LookupBase;

public static class ThrowHandednessExtensions
{
    public static ThrowHandedness FromCore(this ThrowHandednessDto throwHandednessDto)
    {
        return new ThrowHandedness
        {
            Id = throwHandednessDto.Id,
            Name = throwHandednessDto.Name
        };
    }
}