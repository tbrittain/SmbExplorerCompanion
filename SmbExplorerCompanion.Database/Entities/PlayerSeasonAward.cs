using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSeasonAward
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int PlayerAwardId { get; set; }
    public virtual PlayerAward PlayerAward { get; set; } = default!;
}