using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSecondaryPositionHistory
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int PositionId { get; set; }
    public virtual Position Position { get; set; } = default!;
}