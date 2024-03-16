using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record Trait : LookupBase
{
    public bool IsSmb3 { get; set; }
    public bool IsPositive { get; set; }
}

public static class TraitExtensions
{
    public static Trait FromCore(this TraitDto traitDto)
    {
        return new Trait
        {
            Id = traitDto.Id,
            Name = traitDto.Name,
            IsSmb3 = traitDto.IsSmb3,
            IsPositive = traitDto.IsPositive
        };
    }
}
