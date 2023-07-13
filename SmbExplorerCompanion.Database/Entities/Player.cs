using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Entities;

public class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsHallOfFamer { get; set; }
    public virtual BatHandedness BatHandedness { get; set; } = default!;
    public virtual ThrowHandedness ThrowHandedness { get; set; } = default!;
    public virtual Position PrimaryPosition { get; set; } = default!;
    public virtual PitcherRole? PitcherRole { get; set; }
    public virtual Chemistry? Chemistry { get; set; }
    public virtual ICollection<PlayerSeason> Seasons { get; set; } = new HashSet<PlayerSeason>();
    public virtual ICollection<PlayerGameIdHistory> GameIdHistory { get; set; } = new HashSet<PlayerGameIdHistory>();
}