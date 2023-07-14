namespace SmbExplorerCompanion.Database.Entities;

public class Season
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    // In theory, this could change if the franchise is migrated to a new league,
    // and the user modifies the number of games per season.
    public int NumGamesRegularSeason { get; set; }

    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
}