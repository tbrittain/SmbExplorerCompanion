namespace SmbExplorerCompanion.Database.Entities;

public class Conference
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsDesignatedHitter { get; set; }

    public int FranchiseId { get; set; }
    public virtual Franchise Franchise { get; set; } = default!;

    public virtual ICollection<Division> Divisions { get; set; } = new HashSet<Division>();
}