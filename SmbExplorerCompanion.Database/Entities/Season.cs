using System.ComponentModel.DataAnnotations.Schema;

namespace SmbExplorerCompanion.Database.Entities;

public class Season
{
    /// <summary>
    ///     We will reuse the SMB Season ID as the primary key for this entity.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int Number { get; set; } = default!;

    // In theory, this could change if the franchise is migrated to a new league,
    // and the user modifies the number of games per season. We will need to compute this value
    // when we import the season schedule data.
    public int NumGamesRegularSeason { get; set; }

    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
    public virtual ICollection<SeasonTeamHistory> SeasonTeamHistory { get; set; } = new HashSet<SeasonTeamHistory>();
}