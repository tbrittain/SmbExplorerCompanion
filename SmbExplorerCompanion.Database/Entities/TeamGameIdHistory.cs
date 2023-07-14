namespace SmbExplorerCompanion.Database.Entities;

/// <summary>
/// Similar to PlayerGameIdHistory, this maps the game's GUID of a team back to a common team in this application,
/// in the case of a franchise being exported to a new league, and then a new franchise being created in that league.
/// </summary>
public class TeamGameIdHistory
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public virtual Team Team { get; set; } = default!;
    public Guid GameId { get; set; }
}