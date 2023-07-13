namespace SmbExplorerCompanion.Database.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int DivisionId { get; set; }
    public virtual Division Division { get; set; } = default!;
    public virtual ICollection<TeamDivisionHistory> TeamDivisionHistory { get; set; } = new HashSet<TeamDivisionHistory>();
    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
}