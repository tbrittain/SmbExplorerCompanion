namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class Position
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    
    public virtual ICollection<Player> PrimaryPositionPlayers { get; set; } = new HashSet<Player>();
    public virtual ICollection<PlayerSeason> SecondaryPositionPlayers { get; set; } = new HashSet<PlayerSeason>();
}