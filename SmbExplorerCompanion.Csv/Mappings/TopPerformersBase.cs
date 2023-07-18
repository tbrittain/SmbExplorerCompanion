namespace SmbExplorerCompanion.Csv.Mappings;

public abstract class TopPerformersBase
{
    public Guid PlayerId { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}