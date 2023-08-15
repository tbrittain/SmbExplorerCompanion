namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamDto
{
    public int SeasonTeamId { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNumber { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string ConferenceName { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
}