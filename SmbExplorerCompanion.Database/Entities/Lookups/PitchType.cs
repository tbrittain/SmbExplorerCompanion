namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class PitchType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    
    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
}