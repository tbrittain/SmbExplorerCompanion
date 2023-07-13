namespace SmbExplorerCompanion.Database.Entities;

public class Franchise
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public virtual ICollection<Season> Seasons { get; set; } = new HashSet<Season>();
    public virtual ICollection<Conference> Conferences { get; set; } = new HashSet<Conference>();
}