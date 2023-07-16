namespace SmbExplorerCompanion.Database.Entities;

public class Franchise
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsSmb3 { get; set; }
    
    public virtual ICollection<Conference> Conferences { get; set; } = new HashSet<Conference>();
}