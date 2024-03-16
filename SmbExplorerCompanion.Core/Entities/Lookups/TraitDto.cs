using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Entities.Lookups;

public class TraitDto : LookupBaseDto
{
    public bool IsSmb3 { get; set; }
    public bool IsPositive { get; set; }
}