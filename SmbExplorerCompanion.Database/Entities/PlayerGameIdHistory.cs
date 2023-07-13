namespace SmbExplorerCompanion.Database.Entities;

/// <summary>
/// This entity tracks the GUID of the given player, which may change in the case
/// of a franchise play through being migrated to a new league, and then a new
/// franchise being created in that league.
/// </summary>
public class PlayerGameIdHistory
{
    public int Id { get; set; }
    public virtual Player Player { get; set; } = default!;
    public Guid GameId { get; set; }
}