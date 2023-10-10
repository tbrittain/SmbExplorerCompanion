namespace SmbExplorerCompanion.Database.Entities;

public class TeamNameHistory
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public int? TeamLogoHistoryId { get; set; }
    public virtual TeamLogoHistory? TeamLogoHistory { get; set; } = default!;

    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
}