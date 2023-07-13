namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class Chemistry
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public virtual ICollection<Trait> Traits { get; set; } = new HashSet<Trait>();
}