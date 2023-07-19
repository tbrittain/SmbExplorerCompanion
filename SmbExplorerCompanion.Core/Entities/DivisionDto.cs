namespace SmbExplorerCompanion.Core.Entities;

public class DivisionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<TeamDto> Teams { get; set; } = new();
}