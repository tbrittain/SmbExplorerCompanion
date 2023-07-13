namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class BatHandedness
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public virtual ICollection<Player> Players { get; set; } = new HashSet<Player>();
}