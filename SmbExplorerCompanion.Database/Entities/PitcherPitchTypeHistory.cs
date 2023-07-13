using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Entities;

public class PitcherPitchTypeHistory
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int PitchTypeId { get; set; }
    public virtual PitchType PitchType { get; set; } = default!;
}