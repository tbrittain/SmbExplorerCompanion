namespace SmbExplorerCompanion.Csv.Mappings;

public abstract class SeasonStatBase
{
    public Guid PlayerId { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? CurrentTeam { get; set; } = default!;
    public Guid? CurrentTeamId { get; set; }
    public string? PreviousTeam { get; set; } = default!;
    public Guid? PreviousTeamId { get; set; }
    public string? SecondPreviousTeam { get; set; } = default!;
    public Guid? SecondPreviousTeamId { get; set; }
}