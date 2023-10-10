namespace SmbExplorerCompanion.Database.Entities;

public class ChampionshipWinner
{
    public int Id { get; set; }
    public int SeasonTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory SeasonTeamHistory { get; set; } = default!;
    public int SeasonId { get; set; }
    public virtual Season Season { get; set; } = default!;
    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
}