namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class Trait
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsSmb3 { get; set; }
    public bool IsPositive { get; set; } = true;
    public int? ChemistryId { get; set; }
    public virtual Chemistry? Chemistry { get; set; }
}