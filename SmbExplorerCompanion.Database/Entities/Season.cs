namespace SmbExplorerCompanion.Database.Entities;

public class Season
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int NumGames { get; set; }
    public int FranchiseId { get; set; }
    public virtual Franchise Franchise { get; set; } = default!;
    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
}