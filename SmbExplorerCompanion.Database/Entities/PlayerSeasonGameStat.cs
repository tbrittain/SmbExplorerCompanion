namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSeasonGameStat
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int Power { get; set; }
    public int Contact { get; set; }
    public int Speed { get; set; }
    public int Fielding { get; set; }
    public int? Arm { get; set; }
    public int? Velocity { get; set; }
    public int? Junk { get; set; }
    public int? Accuracy { get; set; }
}