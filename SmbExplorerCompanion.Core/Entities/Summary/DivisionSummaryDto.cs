namespace SmbExplorerCompanion.Core.Entities.Summary;

public class DivisionSummaryDto
{
    public int Id { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public List<TeamSummaryDto> Teams { get; set; } = new();
}