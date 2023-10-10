using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Entities;

public class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsHallOfFamer { get; set; }

    public int BatHandednessId { get; set; }
    public virtual BatHandedness BatHandedness { get; set; } = default!;

    public int ThrowHandednessId { get; set; }
    public virtual ThrowHandedness ThrowHandedness { get; set; } = default!;

    public int PrimaryPositionId { get; set; }
    public virtual Position PrimaryPosition { get; set; } = default!;

    public int? PitcherRoleId { get; set; }
    public virtual PitcherRole? PitcherRole { get; set; }

    public int? ChemistryId { get; set; }
    public virtual Chemistry? Chemistry { get; set; }
    
    public int FranchiseId { get; set; }
    public virtual Franchise Franchise { get; set; } = default!;

    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
    public virtual ICollection<PlayerGameIdHistory> PlayerGameIdHistory { get; set; } = new HashSet<PlayerGameIdHistory>();
}