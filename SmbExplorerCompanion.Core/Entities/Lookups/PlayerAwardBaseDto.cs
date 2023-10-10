using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Entities.Lookups;

public class PlayerAwardBaseDto : LookupBaseDto
{
    public int Importance { get; set; }
    public bool OmitFromGroupings { get; set; }
}