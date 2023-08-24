namespace SmbExplorerCompanion.Core.Entities.Summary;

public class ConferenceSummaryDto
{
    public int Id { get; set; }
    public string ConferenceName { get; set; } = string.Empty;
    public List<DivisionSummaryDto> Divisions { get; set; } = new();
}