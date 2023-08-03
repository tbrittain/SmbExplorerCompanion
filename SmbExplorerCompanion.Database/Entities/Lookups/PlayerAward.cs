namespace SmbExplorerCompanion.Database.Entities.Lookups;

public class PlayerAward
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string OriginalName { get; set; } = default!;
    public bool IsBuiltIn { get; set; }

    /// <summary>
    ///     The importance of the award. 0 is the most important, and the higher the number, the less important the award.
    ///     General guidelines: 0 = MVP, 1 = Cy Young, Silver Slugger, 2 = Gold Glove, Playoff MVP, Championship MVP, 3 =
    ///     All-Star
    /// </summary>
    public int Importance { get; set; }

    public virtual ICollection<PlayerSeason> PlayerSeasons { get; set; } = new HashSet<PlayerSeason>();
    public bool OmitFromGroupings { get; set; }
}