namespace SmbExplorerCompanion.Database.Entities;

public class Division
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int ConferenceId { get; set; }
    public virtual Conference Conference { get; set; } = default!;

    public virtual ICollection<TeamDivisionHistory> TeamDivisionHistory { get; set; } =
        new HashSet<TeamDivisionHistory>();
}