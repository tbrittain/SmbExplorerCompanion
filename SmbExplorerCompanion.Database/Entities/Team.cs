namespace SmbExplorerCompanion.Database.Entities;

public class Team
{
    public int Id { get; set; }
    
    public int FranchiseId { get; set; }
    public virtual Franchise Franchise { get; set; } = default!;
    public virtual ICollection<TeamGameIdHistory> TeamGameIdHistory { get; set; } = new HashSet<TeamGameIdHistory>();
    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
}