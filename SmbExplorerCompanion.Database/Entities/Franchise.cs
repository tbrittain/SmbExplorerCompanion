namespace SmbExplorerCompanion.Database.Entities;

public class Franchise
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsSmb3 { get; set; } = false;
    public virtual ICollection<Season> Seasons { get; set; } = new HashSet<Season>();
    public virtual ICollection<Conference> Conferences { get; set; } = new HashSet<Conference>();
}