namespace SmbExplorerCompanion.Core.Entities.Lookups;

public class PlayerAwardBase
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Importance { get; set; }
    public bool OmitFromGroupings { get; set; }
}