namespace SmbExplorerCompanion.Database.Entities;

/// <summary>
/// Although unlikely to change under normal circumstances, this entity tracks
/// if a team is switched divisions, which may be the case if a franchise
/// is exported to a new league, and then the user swaps the team to a different
/// division.
/// </summary>
public class TeamDivisionHistory
{
    public int Id { get; set; }
    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
    public int DivisionId { get; set; }
    public virtual Division Division { get; set; } = default!;
}