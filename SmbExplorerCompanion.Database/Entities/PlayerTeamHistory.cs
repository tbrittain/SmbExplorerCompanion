namespace SmbExplorerCompanion.Database.Entities;

public class PlayerTeamHistory
{
    public int Id { get; set; }
    public int PlayerSeasonId { get; set; }
    public virtual PlayerSeason PlayerSeason { get; set; } = default!;
    public int? SeasonTeamHistoryId { get; set; }
    public virtual SeasonTeamHistory? SeasonTeamHistory { get; set; }

    /// <summary>
    ///     Order represents the index of the team in the player's history. So, if a player has played for 3 teams,
    ///     the current (including no team) will have an order of 1, the most recent/previous will have an order of 2,
    ///     and the previous to that will have an order of 3. Unfortunately, we do not have any further precision
    ///     than this with regard to determining which games were played for which team, and similarly
    ///     the stats breakdown for each team.
    /// </summary>
    public int Order { get; set; }
}