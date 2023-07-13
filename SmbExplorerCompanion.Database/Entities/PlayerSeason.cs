namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSeason
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = default!;
    public int SeasonId { get; set; }
    public virtual Season Season { get; set; } = default!;
    public int Age { get; set; }
    public int Salary { get; set; }

    public virtual ICollection<PitcherPitchTypeHistory> PitcherPitchTypeHistory { get; set; } =
        new HashSet<PitcherPitchTypeHistory>();

    public virtual ICollection<PlayerSecondaryPositionHistory> SecondaryPositionHistory { get; set; } =
        new HashSet<PlayerSecondaryPositionHistory>();

    public virtual ICollection<PlayerTeamHistory> TeamHistory { get; set; } =
        new HashSet<PlayerTeamHistory>();
    
    // TODO: gamestats, battingstats, pitchingstats
}