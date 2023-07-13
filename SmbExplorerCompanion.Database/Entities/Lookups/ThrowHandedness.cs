namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class ThrowHandedness
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public virtual ICollection<Player> Players { get; set; } = new HashSet<Player>();
}