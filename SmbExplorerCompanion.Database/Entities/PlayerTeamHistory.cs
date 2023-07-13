namespace SmbExplorerCompanion.Database.Entities;

public class PlayerTeamHistory
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int SeasonTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory SeasonTeamHistory { get; set; } = default!;
    public int Order { get; set; }
}