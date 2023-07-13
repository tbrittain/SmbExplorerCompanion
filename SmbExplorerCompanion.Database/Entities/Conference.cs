namespace SmbExplorerCompanion.Database.Entities;

public class Conference
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsDesignatedHitter { get; set; }
    public virtual ICollection<Division> Divisions { get; set; } = new HashSet<Division>();
}